using Lucent.Client;
using Lucent.Core;
using Microsoft.Data.SqlClient;
using System.Text.Json.Nodes;

namespace Lucent.Loader;

public sealed class LucentWorker : BackgroundService, ILucentLoader
{
    private readonly ILucentClient _client;
    private readonly IConfiguration _cfg;
    private readonly ILogger<LucentWorker> _log;

    public LucentWorker(ILucentClient client, IConfiguration cfg, ILogger<LucentWorker> log)
    {
        _client = client;
        _cfg = cfg;
        _log = log;
    }

    // Implement the LoadDataAsync method from ILucentLoader
    public async Task LoadDataAsync(Guid tenantId, string ep, DateTime asAt, SqlConnection conn, CancellationToken ct)
    {
        _log.LogInformation("Fetching {Endpoint}", ep);

        int pages = 0;
        await foreach (var (pageNo, json) in
            _client.FetchAllPagesAsync(ep, null, tenantId, ct)) // TenantId passed as Guid
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

        if (pages == 0) return; // nothing new

        // Execute the stored procedure and update control tables
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

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var endpoints = _cfg.GetSection("Lucent:Endpoints").Get<string[]>()
                        ?? Array.Empty<string>();

        Guid tenantId;
        var tenantCfg = _cfg["Lucent:TenantId"];

        if (!Guid.TryParse(tenantCfg, out tenantId))
        {
            _log.LogInformation("TenantId missing/invalid – discovering via /connections…");
            tenantId = await _client.DiscoverFirstTenantIdAsync(ct);

            var cfgPath = ConfigPathHelper.GetSharedConfigPath();
            var json = JsonNode.Parse(await File.ReadAllTextAsync(cfgPath, ct))!;
            json["Lucent"] ??= new JsonObject();
            json["Lucent"]!["TenantId"] = tenantId.ToString();

            await File.WriteAllTextAsync(cfgPath, json.ToJsonString(new() { WriteIndented = true }), ct);
            _log.LogInformation("Persisted TenantId {Tenant} to {File}", tenantId, cfgPath);
        }

        var connStr = _cfg.GetConnectionString("Sql")!;
        await using var conn = new SqlConnection(connStr);
        await conn.OpenAsync(ct);

        // Loop over each endpoint and call LoadDataAsync
        foreach (var ep in endpoints)
        {
            DateTime? since = await new SqlCommand(@"
                    SELECT ModifiedSinceUTC
                    FROM   dbo.EndpointLoadControl
                    WHERE  EndpointName = @ep;",
                    conn)
            { Parameters = { new("@ep", ep) } }
            .ExecuteScalarAsync(ct) as DateTime?;

            // Now, directly call LoadDataAsync with the correct type (Guid)
            await LoadDataAsync(tenantId, ep, since ?? DateTime.UtcNow,  conn, ct);
        }
    }
}
