// Lucent.Scheduler / LoadJob.cs
using System.Diagnostics;
using Lucent.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Lucent.Scheduler;

public sealed class LoadJob : IJob
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<LoadJob> _log;

    public LoadJob(IConfiguration cfg, ILogger<LoadJob> log)
    {
        _cfg = cfg;
        _log = log;
    }

    public async Task Execute(IJobExecutionContext context)        // ← async
    {
        var sched = _cfg.GetRequiredSection("Schedule");

        /* ── 1. read schedule parameters ──────────────────────── */
        var tenants = sched.GetSection("Tenants").Get<string[]>()
                     ?? throw new InvalidOperationException("Schedule:Tenants array missing");

        var fyStart = DateTime.Parse(
                          sched["FyStart"] ?? throw new InvalidOperationException("FyStart missing"));

        DateTime? horizon = string.IsNullOrWhiteSpace(sched["Horizon"])
                            ? null
                            : DateTime.Parse(sched["Horizon"]!);

        /* ── 2. loop over tenants & periods ───────────────────── */
        foreach (var tenantId in tenants)
        {
            _log.LogInformation("Nightly load for {Tenant} (FY start {Start:d}, horizon {Hz})",
                                tenantId, fyStart, horizon);

            // TB & BS month-ends
            foreach (var asAt in ReportPeriodGenerator.MonthEnds(fyStart, horizon))
            {
                await LaunchLoaderAsync($"--tenant {tenantId} --report TrialBalance --asAt {asAt:yyyy-MM-dd}");
                await LaunchLoaderAsync($"--tenant {tenantId} --report BalanceSheet --asAt {asAt:yyyy-MM-dd}");
            }

            // P&L month slices
            foreach (var (from, to) in ReportPeriodGenerator.PLMonths(fyStart, horizon))
            {
                await LaunchLoaderAsync($"--tenant {tenantId} --report ProfitAndLoss --from {from:yyyy-MM-dd} --to {to:yyyy-MM-dd}");
            }
        }
    }   // ← missing brace was here

    /* helper: fire Lucent.Loader once per report ------------------------ */
    private static Task LaunchLoaderAsync(string args)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "Lucent.Loader.exe",      // Scheduler & Loader side-by-side
            Arguments = args,
            CreateNoWindow = true,
            UseShellExecute = false
        });

        return Task.CompletedTask;               // fire-and-forget
    }
}
