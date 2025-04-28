using System;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Lucent.Scheduler;

public sealed class RunNow : IRunNow
{
    private readonly ISchedulerFactory _factory;
    private readonly ILogger<RunNow> _log;
    private static readonly JobKey LoadJobKey = new("NightlyLoad");

    public RunNow(ISchedulerFactory factory, ILogger<RunNow> log)
    {
        _factory = factory;
        _log = log;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var scheduler = await _factory.GetScheduler(ct);

        // ── correlation-id for this manual run ───────────────
        var corrId = Guid.NewGuid().ToString("N")[..8];
        var data = new JobDataMap
        {
            ["TriggeredBy"] = "RunNow",
            ["CorrelationId"] = corrId
        };

        _log.LogInformation("Run-Now: triggering {Job}. CorrelationId={C}",
                            LoadJobKey, corrId);

        await scheduler.TriggerJob(LoadJobKey, data, ct);    // ← pass the map

        _log.LogInformation("Run-Now: job en-queued. CorrelationId={C}", corrId);
    }
}
