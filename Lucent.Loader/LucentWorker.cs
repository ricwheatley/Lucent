using System.IO;
using System.Text.Json.Nodes;
using Lucent.Client;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lucent.Loader;

public sealed class LucentWorker : BackgroundService
{
    private readonly ILucentClient _client;
    private readonly IConfiguration _cfg;
    private readonly ILogger<LucentWorker> _log;

    public LucentWorker(ILucentClient client,
                        IConfiguration cfg,
                        ILogger<LucentWorker> log)
    {
        _client = client;
        _cfg = cfg;
        _log = log;
    }

    private static string GetSharedConfigPath()
    => Path.GetFullPath(Path.Combine(
           AppContext.BaseDirectory, "..", "..", "..", "..",
           "config", "appsettings.json"));

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        /* ──1. endpoints list ─────────────────────────────────────────── */
        var endpoints = _cfg.GetSection("Lucent:Endpoints").Get<string[]>()
                        ?? Array.Empty<string>();

        /* ──2. resolve tenant-id (discover once, then cache) ─────────── */
        Guid tenantId;
        var tenantCfg = _cfg["Lucent:TenantId"];

        if (!Guid.TryParse(tenantCfg, out tenantId))
        {
            _log.LogInformation("TenantId missing/invalid – discovering via /connections…");
            tenantId = await _client.DiscoverFirstTenantIdAsync(ct);

            // write back so next run is fast
            var cfgPath = GetSharedConfigPath();          // ← unified path
            var json = JsonNode.Parse(await File.ReadAllTextAsync(cfgPath, ct))!;
            json["Lucent"] ??= new JsonObject();          // ensure section exists
            json["Lucent"]!["TenantId"] = tenantId.ToString();

            await File.WriteAllTextAsync(
                cfgPath,
                json.ToJsonString(new() { WriteIndented = true }),
                ct);

            _log.LogInformation("Persisted TenantId {Tenant} to {File}", tenantId, cfgPath);
        }

        /* ──3. open SQL connection ───────────────────────────────────── */
        var connStr = _cfg.GetConnectionString("Sql")!;
        await using var conn = new SqlConnection(connStr);
        await conn.OpenAsync(ct);

        /* ──4. process each endpoint ─────────────────────────────────── */
        foreach (var ep in endpoints)
        {
            _log.LogInformation("Fetching {Endpoint}", ep);

            // read watermark
            DateTime? since = await new SqlCommand(@"
                    SELECT ModifiedSinceUTC
                    FROM   dbo.EndpointLoadControl
                    WHERE  EndpointName = @ep;",
                    conn)
            { Parameters = { new("@ep", ep) } }
            .ExecuteScalarAsync(ct) as DateTime?;

            int pages = 0;
            await foreach (var (pageNo, json) in
                _client.FetchAllPagesAsync(ep, since, tenantId, ct))
            {
                pages++;

                await new SqlCommand($@"
                    INSERT INTO stg.[{ep}] (RawJson, PageNumber, SourceEndpoint)
                    VALUES (@j, @p, @e);",
                    conn)
                {
                    Parameters =
                    {
                        new("@j", json),
                        new("@p", pageNo),
                        new("@e", ep)
                    }
                }.ExecuteNonQueryAsync(ct);
            }

            if (pages == 0) continue;   // nothing new

            await new SqlCommand(
                "EXEC dbo.usp_MergeLandingToODS @EndpointName=@ep;",
                conn)
            { Parameters = { new("@ep", ep) } }
            .ExecuteNonQueryAsync(ct);

            await new SqlCommand(@"
                UPDATE dbo.EndpointLoadControl
                SET ModifiedSinceUTC = SYSUTCDATETIME(),
                    LastSuccessUTC   = SYSUTCDATETIME()
                WHERE EndpointName = @ep;",
                conn)
            { Parameters = { new("@ep", ep) } }
            .ExecuteNonQueryAsync(ct);

            _log.LogInformation("{Endpoint}: {Pages} page(s) merged.", ep, pages);
        }
    }
}
