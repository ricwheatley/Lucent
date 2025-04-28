using Lucent.Auth;
using Lucent.Client;
using Lucent.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Lucent.Scheduler;

public static class SchedulerServiceCollectionExtensions
{
    public static IServiceCollection AddLucentScheduler(this IServiceCollection services)
    {
        /* 1 ─ core Lucent deps ------------------------------------------ */
        services.AddOptions();                              // IConfiguration in DI
        services.AddSingleton<ILucentAuth, LucentAuth>();
        services.AddSingleton<ILucentClient, XeroApiClient>();
        services.AddSingleton<ILucentLoader, LucentWorker>();

        /* 2 ─ read RunTime once and build cron -------------------------- */
        var cfg = services.BuildServiceProvider()
                            .GetRequiredService<IConfiguration>();
        var runAt = cfg["Schedule:RunTime"] ?? "01:00";     // HH:mm

        if (!TimeSpan.TryParse(runAt, out var t))
            throw new FormatException($"Schedule:RunTime '{runAt}' is not HH:mm");

        var cron = $"0 {t.Minutes} {t.Hours} ? * *";       // sec min hour dom mon dow

        /* 3 ─ Quartz job + trigger ------------------------------------- */
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("NightlyLoad");

            q.AddJob<LoadJob>(o => o.WithIdentity(jobKey));

            q.AddTrigger(tg => tg.ForJob(jobKey)
                                 .WithCronSchedule(
                                     cron,
                                     x => x.InTimeZone(
                                         TimeZoneInfo.FindSystemTimeZoneById("Europe/London"))));
        });

        services.AddQuartzHostedService();

        /* 4 ─ Run-now helper ------------------------------------------- */
        services.AddSingleton<IRunNow, RunNow>();

        return services;
    }
}
