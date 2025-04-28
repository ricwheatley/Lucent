using Lucent.Core;
using Lucent.Core.Loaders;
using Lucent.Scheduler;
using Lucent.Api;                 // RunRequest / RunStatus
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Lucent.Core.Scheduling;
using System.Text.Json;         // for validation, optional

var builder = WebApplication.CreateBuilder(args);

/* ───── configuration & logging ───────────────────────────── */
var sharedCfg = ConfigPathHelper.GetSharedConfigPath();

var schedPath = Path.Combine(
                Path.GetDirectoryName(sharedCfg)!,
                "tenant-schedule.json");

builder.Configuration.AddJsonFile(sharedCfg, optional: false, reloadOnChange: true);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
    o.IncludeScopes = true;   // shows “=> CorrelationId: ab12cd34”
});

/* ───── services ──────────────────────────────────────────── */
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ILucentLoader, NoOpLucentLoader>();

builder.Services.AddLucentScheduler();          // extension in Lucent.Scheduler
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

/* minimal “old” run-now (kept for backward compatibility) */
app.MapPost("/run-now", async (IRunNow runner, ILoggerFactory lf, CancellationToken ct) =>
{
    var log = lf.CreateLogger("Api");
    log.LogInformation("POST /run-now received");
    await runner.RunAsync(ct);
    return Results.Ok(new { ok = true });
})
.WithName("RunNow")
.Produces(200)
.WithOpenApi();

/* POST /runs  – queue a parameterised run */
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

/* GET /runs/{id}  – polling endpoint for the UI */
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


/* GET  /schedule  – return the whole grid */
app.MapGet("/schedule", async (ITenantScheduleStore store, CancellationToken ct)
    => Results.Ok(await store.LoadAsync(ct)))
   .WithName("GetSchedule")
   .Produces<IReadOnlyList<TenantSchedule>>(200)
   .WithOpenApi();

/* PUT /schedule  – replace everything */
app.MapPut("/schedule", async (
        List<TenantSchedule> schedules,
        ITenantScheduleStore store,
        ILogger<Program> log,
        CancellationToken ct) =>
{
    /* quick sanity-check: duplicate tenant IDs? */
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
