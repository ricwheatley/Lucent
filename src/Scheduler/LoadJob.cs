
using System.Data;
using Lucent.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Lucent.Scheduler;

public sealed class LoadJob : IJob
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<LoadJob> _log;
    private readonly ILucentLoader _loader;
    private readonly ITenantScheduleStore _store;

    public LoadJob(IConfiguration cfg,
                   ILogger<LoadJob> log,
                   ILucentLoader loader,
                   ITenantScheduleStore store)
    {
        _cfg = cfg;
        _log = log;
        _loader = loader;
        _store = store;
    }

    public async Task Execute(IJobExecutionContext ctx)
    {
        /* ───── 0. correlation & trigger info ─────────────────────────── */
        var corrId = ctx.MergedJobDataMap.TryGetValue("CorrelationId", out var idObj)
                   ? idObj!.ToString()
                   : Guid.NewGuid().ToString("N")[..8];

        var triggeredBy = ctx.MergedJobDataMap.TryGetValue("TriggeredBy", out var byObj)
                        ? byObj!.ToString()
                        : "Schedule";

        /* ───── 1. hydrate run-status registry (if present) ───────────── */
        if (ctx.Scheduler.Context.Get("RunRegistry") is RunRegistry reg)
            reg.Set(corrId!, RunStatus.Running);

        using (_log.BeginScope(new Dictionary<string, object?> { ["CorrelationId"] = corrId }))
        {
            _log.LogInformation("▶ LoadJob start. TriggeredBy={By}", triggeredBy);

            /* ───── 2. fetch schedule rows from DB / cache  ───────────── */
            var schedules = (await _store.LoadAsync(ctx.CancellationToken))
                            .ToDictionary(t => t.TenantId);

            /* global default endpoint list comes from secrets / env-vars */
            var defaultEndpoints = _cfg.GetSection("Lucent:Endpoints")
                                       .Get<string[]>()
                                   ?? Array.Empty<string>();

            /* ───── 3. open SQL once per job ──────────────────────────── */
            await using var conn =
                new SqlConnection(_cfg.GetConnectionString("Sql")!);
            await conn.OpenAsync(ctx.CancellationToken);

            /* 4 – loop tenants & endpoints ---------------------------------- */
            foreach (var ts in schedules.Values)
            {
                _log.LogInformation("Tenant {Tenant}", ts.TenantId);

                // if the user unticks everything we still grab Organisation
                HashSet<string> activeEp =
                    ts.EnabledEndpoints?.Count > 0
                        ? ts.EnabledEndpoints
                        : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Organisation" };

                await _loader.RunAsync(ctx.CancellationToken);

            }

        }
            /* ───── 5. final status ──────────────────────────────────────── */
            if (ctx.Scheduler.Context.Get("RunRegistry") is RunRegistry reg2)
            reg2.Set(corrId!, RunStatus.Succeeded);
    }
}
