// Lucent.Loader / LucentWorker.cs  – refactored, no JSON writes
using System.Data;
using Lucent.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using Lucent.Infrastructure;


namespace Lucent.Loader;

public sealed class LucentWorker : BackgroundService, ILucentLoader
{
    private readonly ILucentClient _client;
    private readonly IConfiguration _cfg;
    private readonly ILogger<LucentWorker> _log;

    public LucentWorker(
        ILucentClient client,
        IConfiguration cfg,
        ILogger<LucentWorker> log)
    {
        _client = client;
        _cfg = cfg;
        _log = log;
    }

    /* ------------------------------------------------------------------
       ILucentLoader implementation
    ------------------------------------------------------------------- */
    public async Task LoadDataAsync(
        Guid tenantId,
        string ep,
        DateTime asAt,
        SqlConnection conn,
        CancellationToken ct)
    {
        _log.LogInformation("Fetching {Endpoint}", ep);

        int pages = 0;
        await foreach (var (pageNo, json) in
            _client.FetchAllPagesAsync(ep, null, tenantId, ct))
        {
            pages++;

            await new SqlCommand($@"
                    INSERT INTO stg.[{ep}] (RawJson, PageNumber, SourceEndpoint)
                    VALUES (@j, @p, @e);", conn)
            {
                Parameters =
                {
                    new("@j", json),
                    new("@p", pageNo),
                    new("@e", ep)
                }
            }.ExecuteNonQueryAsync(ct);
        }

        if (pages == 0) return;   // nothing new

        await new SqlCommand(
            "EXEC dbo.usp_MergeLandingToODS @EndpointName=@ep;",
            conn)
        { Parameters = { new("@ep", ep) } }
        .ExecuteNonQueryAsync(ct);

        await new SqlCommand(@"
                UPDATE dbo.EndpointLoadControl
                SET ModifiedSinceUTC = SYSUTCDATETIME(),
                    LastSuccessUTC   = SYSUTCDATETIME()
                WHERE EndpointName = @ep;", conn)
        { Parameters = { new("@ep", ep) } }
        .ExecuteNonQueryAsync(ct);

        _log.LogInformation("{Endpoint}: {Pages} page(s) merged.", ep, pages);
    }

    /* ------------------------------------------------------------------
       BackgroundService entry point
    ------------------------------------------------------------------- */
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var endpoints = _cfg.GetSection("Lucent:Endpoints").Get<string[]>()
                        ?? Array.Empty<string>();

        /* ───── resolve TenantId ───────────────────────────────── */
        Guid tenantId;
        if (!Guid.TryParse(_cfg["Lucent:TenantId"], out tenantId))
        {
            _log.LogInformation("TenantId missing/invalid – discovering via /connections…");
            tenantId = await _client.DiscoverFirstTenantIdAsync(ct);
            await PersistTenantAsync(tenantId, ct);   // ensure row in schedule
        }

        var connStr = _cfg.GetConnectionString("Sql")
                      ?? throw new InvalidOperationException("Connection string 'Sql' missing.");
        await using var conn = new SqlConnection(connStr);
        await conn.OpenAsync(ct);

        foreach (var ep in endpoints)
        {
            var since = await new SqlCommand(@"
                        SELECT ModifiedSinceUTC
                        FROM   dbo.EndpointLoadControl
                        WHERE  EndpointName = @ep;", conn)
            { Parameters = { new("@ep", ep) } }
            .ExecuteScalarAsync(ct) as DateTime?;

            await LoadDataAsync(tenantId, ep, since ?? DateTime.UtcNow, conn, ct);
        }
    }

    public async Task<LoadResult> RunAsync(CancellationToken ct = default)
    {
        try
        {
            await ExecuteAsync(ct);          // reuse pipeline above
            return new LoadResult(true);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Loader run failed");
            return new LoadResult(false, ex.Message);
        }
    }

    /* ------------------------------------------------------------------
       Insert unseen tenant into dbo.Organisations
    ------------------------------------------------------------------- */
    private async Task PersistTenantAsync(Guid tenantId, CancellationToken ct)
    {
        var connStr = _cfg.GetConnectionString("Sql")
                      ?? throw new InvalidOperationException("Connection string 'Sql' missing.");
        await using var conn = new SqlConnection(connStr);
        await conn.OpenAsync(ct);

        const string sql = @"
        /* 1 – fetch friendly name from ods if present */
        DECLARE @name nvarchar(100);
        SELECT @name = Name
        FROM   ods.Organisations
        WHERE  TenantId = @tid;

        /* 2 – insert into schedule only if missing */
        IF NOT EXISTS (SELECT 1 FROM dbo.TenantSchedule WHERE TenantId = @tid)
        BEGIN
            INSERT INTO dbo.TenantSchedule
                (TenantId, TenantName, RunTime, StartDate, EndDate, EnabledEndpoints)
            VALUES
                (@tid,
                 COALESCE(@name, 'New tenant'),
                 '01:00',
                 '2000-01-01', '2999-12-31',
                 '[]');
        END";

        await new SqlCommand(sql, conn)
        {
            Parameters = { new("@tid", tenantId) }
        }.ExecuteNonQueryAsync(ct);

        _log.LogInformation("Tenant {Tenant} ensured in dbo.TenantSchedule", tenantId);
    }
}
