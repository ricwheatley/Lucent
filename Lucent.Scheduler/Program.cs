// Lucent.Scheduler / Program.cs  – FIXED
using Lucent.Core;
using Lucent.Scheduler;
using Lucent.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Lucent.Auth;
using Lucent.Client;
using Lucent.Resilience;          // RetryPolicies
using Polly.Registry;
using RestSharp;
using System.Net.Http;            // IHttpClientFactory


var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
    o.IncludeScopes = true;           // ★ shows “=> CorrelationId: ab12cd34”
});

/* 1 ─ shared configuration -------------------------------------------- */
builder.Configuration.AddJsonFile(
    ConfigPathHelper.GetSharedConfigPath(), optional: false, reloadOnChange: true);

/* 1a ─ resilience: Polly registry + retry-enabled client */
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

builder.Services.AddScoped<ILucentLoader, LucentWorker>();  // Register the interface and implementation
builder.Services.AddScoped<ILucentAuth, LucentAuth>();         //  Register the Auth
builder.Services.AddScoped<ILucentClient, XeroApiClient>();

/* 2 ─ read run-time BEFORE host is built ------------------------------ */
var runAt = TimeOnly.Parse(builder.Configuration["Schedule:RunTime"] ?? "01:00");
string cron = $"0 {runAt.Minute} {runAt.Hour} * * ?";   // daily hh:mm
//string cron = "0/15 * * * * ?"; << -- every 15 seconds for testing
/* 3 ─ logging + Quartz ------------------------------------------------ */
builder.Services.AddLogging(c => c.AddConsole());

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("NightlyLoad");
    q.AddJob<LoadJob>(o => o.WithIdentity(jobKey));

    q.AddTrigger(t => t.ForJob(jobKey)
                       .WithCronSchedule(cron));
});
builder.Services.AddQuartzHostedService();

builder.Services.AddSingleton<IRunNow, RunNow>();

/* 4 ─ run -------------------------------------------------------------- */
await builder.Build().RunAsync();
