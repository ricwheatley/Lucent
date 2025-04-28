using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Lucent.Core;
using Lucent.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using Lucent.Core.Scheduling;

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
        /* ────────────────────────── 0. correlation + trigger ───────────────────────── */
        var corrId = ctx.MergedJobDataMap.TryGetValue("CorrelationId", out var idObj)
                   ? idObj!.ToString()
                   : Guid.NewGuid().ToString("N")[..8];

        var triggeredBy = ctx.MergedJobDataMap.TryGetValue("TriggeredBy", out var byObj)
                        ? byObj!.ToString()
                        : "Schedule";
       
        var schedules = (await _store.LoadAsync(ctx.CancellationToken))
                .ToDictionary(t => t.TenantId);

        /* mark “Running” as soon as the job starts */
        if (ctx.Scheduler.Context.Get("RunRegistry") is RunRegistry reg)
            reg.Set(corrId!, RunStatus.Running);

        using (_log.BeginScope(new Dictionary<string, object?> { ["CorrelationId"] = corrId }))
        {
            _log.LogInformation("▶ LoadJob start. TriggeredBy={By}", triggeredBy);

            /* 1 – read schedule parameters */
            var sched = _cfg.GetRequiredSection("Schedule");
            var tenants = sched.GetSection("Tenants").Get<string[]>()
                           ?? throw new InvalidOperationException("Schedule:Tenants?");
            var endpoints = _cfg.GetSection("Lucent:Endpoints").Get<string[]>()
                           ?? throw new InvalidOperationException("Lucent:Endpoints?");

            /* 2 – open SQL once per job */
            await using var conn =
                new SqlConnection(_cfg.GetConnectionString("Sql")!);
            await conn.OpenAsync(ctx.CancellationToken);

            /* 3 – loop tenants & endpoints */
            foreach (var tenant in tenants)
            {
                _log.LogInformation("Load for tenant {Tenant}", tenant);

                var activeEp = schedules.TryGetValue(Guid.Parse(tenant), out var ts)
                             ? ts.EnabledEndpoints
                             : endpoints.ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var ep in activeEp)
                {
                    _log.LogInformation("{Ep}: starting", ep);

                    await _loader.LoadDataAsync(Guid.Parse(tenant),
                                                ep,
                                                DateTime.UtcNow,
                                                conn,
                                                ctx.CancellationToken);

                    _log.LogInformation("{Ep}: done", ep);
                }
            }

            _log.LogInformation("✔ LoadJob finished. TriggeredBy={By}", triggeredBy);
        }

        /* mark final status */
        if (ctx.Scheduler.Context.Get("RunRegistry") is RunRegistry reg2)
            reg2.Set(corrId!, RunStatus.Succeeded);
    }
}
