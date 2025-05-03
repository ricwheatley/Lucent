// Lucent.Scheduler / Program.cs  – secret-first, JSON-free

using Lucent.Core;
using Lucent.Loader;
using Lucent.Infrastructure;
using Lucent.Scheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using Quartz;
using RestSharp;

var builder = Host.CreateApplicationBuilder(args);

/* ── configuration & logging ────────────────────────────────────────── */
builder.Configuration
       .AddJsonFile("config/appsettings.Development.json", optional: true, reloadOnChange: true)
       .AddUserSecrets<Program>(optional: true)
       .AddEnvironmentVariables();

builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Services.AddLogging(c => c.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
    o.IncludeScopes = true;     // shows “=> CorrelationId: ab12cd34”
}));

/* ── resilience: Polly registry + retry-enabled client ─────────────── */
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

/* ── DI for Loader & dependencies ───────────────────────────────────── */
builder.Services.AddScoped<ILucentAuth, LucentAuth>();
builder.Services.AddScoped<ILucentClient, XeroApiClient>();
builder.Services.AddScoped<ILucentLoader, LucentWorker>();
builder.Services.AddSingleton<ITenantScheduleStore, SqlTenantScheduleStore>();

/* ── Quartz scheduling (same cron logic) ───────────────────────────── */
var runAt = TimeOnly.Parse(
                builder.Configuration["Schedule:RunTime"] ?? "01:00");
var cron = $"0 {runAt.Minute} {runAt.Hour} * * ?";  // daily hh:mm

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("NightlyLoad");
    q.AddJob<LoadJob>(o => o.WithIdentity(jobKey));
    q.AddTrigger(t => t.ForJob(jobKey).WithCronSchedule(cron));
});
builder.Services.AddQuartzHostedService();

/* ── run host ───────────────────────────────────────────────────────── */
await builder.Build().RunAsync();
