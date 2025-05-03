using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Lucent.Scheduler;

/// <summary>
/// Manually fires the nightly loader job via Quartz, pushing a correlation
/// identifier into the <see cref="JobDataMap"/> so downstream log entries
/// can be stitched together.
/// </summary>
public sealed class RunNow : IRunNow
{
    // Keep the job key in one place so it cannot drift from the registration
    // in Program.cs.  If your registration uses the default group, drop the
    // second parameter here.
    private static readonly JobKey NightlyLoadKey = new("NightlyLoad", "Loader");

    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<RunNow> _logger;

    public RunNow(
        ISchedulerFactory schedulerFactory,
        ILogger<RunNow> logger)
    {
        _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        // Short GUID keeps log lines readable while remaining unique.
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        var dataMap = new JobDataMap
        {
            ["TriggeredBy"] = nameof(RunNow),
            ["CorrelationId"] = correlationId
        };

        using var _ = _logger.BeginScope(new { CorrelationId = correlationId });

        _logger.LogInformation("Run-Now: en-queuing job {JobKey}.", NightlyLoadKey);

        await scheduler.TriggerJob(NightlyLoadKey, dataMap, cancellationToken);

        _logger.LogInformation("Run-Now: job {JobKey} successfully en-queued.", NightlyLoadKey);
    }
}
