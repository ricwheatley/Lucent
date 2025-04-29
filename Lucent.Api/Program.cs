// Lucent.Api / Program.cs
using Lucent.Api;
using Lucent.Auth;
using Lucent.Auth.TokenCache;
using Lucent.Client;
using Lucent.Core;
using Lucent.Core.Loaders;
using Lucent.Core.Scheduling;
using Lucent.Resilience;
using Lucent.Scheduler;                // AddLucentScheduler, RunRegistry
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using Quartz;
using RestSharp;

var builder = WebApplication.CreateBuilder(args);

/* ───── configuration & logging ───────────────────────────── */

builder.Configuration
       .AddJsonFile("config/appsettings.Development.json", optional: true, reloadOnChange: true)
       .AddUserSecrets<Program>(optional: true)
       .AddEnvironmentVariables();

builder.Services
       .AddOptions<TokenCacheOptions>()
       .Bind(builder.Configuration.GetSection("TokenCache"))
       .ValidateDataAnnotations();

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
    o.IncludeScopes = true;   // shows “=> CorrelationId: ab12cd34”
});

/* ───── resilience: Polly registry + retry-enabled client ─── */

builder.Services.AddSingleton<IPolicyRegistry<string>>(sp =>
{
    var lf = sp.GetRequiredService<ILoggerFactory>();
    return RetryPolicies.BuildRegistry(lf);
});

builder.Services
    .AddHttpClient("LucentHttp")
    .AddPolicyHandlerFromRegistry(RetryPolicies.StandardHttpPolicy);

builder.Services.AddSingleton<IRestClient>(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>()
                 .CreateClient("LucentHttp");
    http.BaseAddress = new Uri("https://identity.xero.com/connect/token");
    return new RestClient(http);
});

/* ───── services ──────────────────────────────────────────── */

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ILucentClient, XeroApiClient>();
builder.Services.AddScoped<ILucentAuth, LucentAuth>();
builder.Services.AddScoped<ILucentLoader, NoOpLucentLoader>();

builder.Services.AddLucentScheduler(builder.Configuration);
builder.Services.AddSingleton<RunRegistry>();   // shared run-state
builder.Services.AddSingleton<ITenantScheduleStore>(
        sp => new SqlTenantScheduleStore(sp.GetRequiredService<IConfiguration>()));

/* CORS so the Runner UI (localhost:5299) can call the API */
builder.Services.AddCors(o =>
    o.AddPolicy("runner", p =>
        p.WithOrigins("http://localhost:5096")
         .AllowAnyHeader()
         .AllowAnyMethod()));

var app = builder.Build();

/* ───── push RunRegistry into Quartz once ─────────────────── */
{
    var registry = app.Services.GetRequiredService<RunRegistry>();
    var scheduler = app.Services.GetRequiredService<ISchedulerFactory>()
                                 .GetScheduler().Result;
    scheduler.Context.Put("RunRegistry", registry);
}

/* ───── middleware ────────────────────────────────────────── */
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("runner");

/* ───── endpoints ─────────────────────────────────────────── */

app.MapPost("/run-now", async (IRunNow runner, ILoggerFactory lf, CancellationToken ct) =>
{
    lf.CreateLogger("Api").LogInformation("POST /run-now received");
    await runner.RunAsync(ct);
    return Results.Ok(new { ok = true });
})
.WithName("RunNow")
.Produces(200)
.WithOpenApi();

app.MapPost("/runs", async (
        RunRequest req,
        RunRegistry reg,
        ISchedulerFactory sf,
        ILogger<Program> log,
        CancellationToken ct) =>
{
    var id = reg.Register();
    reg.Set(id, RunStatus.Queued);

    var data = new JobDataMap
    {
        ["TriggeredBy"] = "RunNow",
        ["CorrelationId"] = id,
        ["StartDate"] = req.StartDate.ToString("o"),
        ["EndDate"] = req.EndDate.ToString("o"),
        ["TenantId"] = req.TenantId.ToString(),
        ["OdsTables"] = req.OdsTables
    };

    var scheduler = await sf.GetScheduler(ct);
    await scheduler.TriggerJob(new JobKey("NightlyLoad"), data, ct);

    log.LogInformation("Run {Id} queued for tenant {T}", id, req.TenantId);
    return Results.Accepted($"/runs/{id}", new { runId = id });
})
.WithName("QueueRun")
.Produces(202)
.WithOpenApi();

app.MapGet("/runs/{id}", (string id, RunRegistry reg) =>
{
    return reg.TryGet(id, out var s)
         ? Results.Ok(new { status = s.ToString() })
         : Results.NotFound();
})
.WithName("RunStatus")
.Produces(200)
.Produces(404)
.WithOpenApi();

app.MapGet("/schedule", async (ITenantScheduleStore store, CancellationToken ct)
    => Results.Ok(await store.LoadAsync(ct)))
   .WithName("GetSchedule")
   .Produces<IReadOnlyList<TenantSchedule>>(200)
   .WithOpenApi();

app.MapPut("/schedule", async (
        List<TenantSchedule> schedules,
        ITenantScheduleStore store,
        ILogger<Program> log,
        CancellationToken ct) =>
{
    if (schedules.GroupBy(s => s.TenantId).Any(g => g.Count() > 1))
        return Results.BadRequest("Duplicate TenantId rows.");

    await store.SaveAsync(schedules, ct);
    log.LogInformation("Tenant schedule updated - {Count} rows.", schedules.Count);
    return Results.NoContent();
})
.WithName("PutSchedule")
.Produces(204)
.Produces(400)
.WithOpenApi();

/* ───── run app ───────────────────────────────────────────── */
app.Run();
