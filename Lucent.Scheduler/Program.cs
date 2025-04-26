// Lucent.Scheduler / Program.cs  – FIXED
using Lucent.Scheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

static string GetSharedConfigPath() =>
    Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..",
        "config", "appsettings.json"));

var builder = Host.CreateApplicationBuilder(args);

/* 1 ─ shared configuration -------------------------------------------- */
builder.Configuration.AddJsonFile(
    GetSharedConfigPath(), optional: false, reloadOnChange: true);

/* 2 ─ read run-time BEFORE host is built ------------------------------ */
var runAt = TimeOnly.Parse(builder.Configuration["Schedule:RunTime"] ?? "01:00");
//string cron = $"0 {runAt.Minute} {runAt.Hour} * * ?";   // daily hh:mm
string cron = "0/15 * * * * ?";
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

/* 4 ─ run -------------------------------------------------------------- */
await builder.Build().RunAsync();
