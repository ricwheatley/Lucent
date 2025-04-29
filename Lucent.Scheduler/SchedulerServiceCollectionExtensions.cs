using System;
using Lucent.Auth;
using Lucent.Client;
using Lucent.Core.Loaders;
using Lucent.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Lucent.Scheduler;

/// <summary>
/// Registers Quartz and the nightly loader job.  
/// Call from Program.cs with:
/// <code>services.AddLucentScheduler(builder.Configuration);</code>
/// </summary>
public static class SchedulerServiceCollectionExtensions
{
    private const string JobName = "NightlyLoad";
    private const string JobGroup = "Loader";
    private static readonly TimeZoneInfo London =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

    public static IServiceCollection AddLucentScheduler(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        /* 1 ── core Lucent dependencies ─────────────────────────────── */
        services.AddOptions();
        services.AddSingleton<ILucentAuth, LucentAuth>();
        services.AddHttpClient<ILucentClient, XeroApiClient>();
        services.AddScoped<ILucentLoader, LucentWorker>();

        /* 2 ── work out the cron expression once ───────────────────── */
        var runAt = configuration["Schedule:RunTime"] ?? "01:00";         // HH:mm

        if (!TimeSpan.TryParse(runAt, out var t))
            throw new FormatException($"Schedule:RunTime '{runAt}' is not a valid HH:mm");

        var cron = $"0 {t.Minutes} {t.Hours} ? * *";                       // sec min hour dom mon dow
        var jobKey = new JobKey(JobName, JobGroup);

        /* 3 ── Quartz registration ─────────────────────────────────── */
        services.AddQuartz(q =>
        {
            q.AddJob<LoadJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity($"{JobName}Trigger", JobGroup)
                .WithCronSchedule(cron, x => x.InTimeZone(London)));
        });

        services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);

        /* 4 ── run-now helper ───────────────────────────────────────── */
        services.AddSingleton<IRunNow, RunNow>();

        return services;
    }
}
