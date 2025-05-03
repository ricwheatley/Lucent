using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Lucent.Core;

public sealed class SqlTenantScheduleStore : ITenantScheduleStore
{
    private readonly string _connStr;
    private readonly JsonSerializerOptions _json =
        new(JsonSerializerDefaults.Web);

    public SqlTenantScheduleStore(IConfiguration cfg)
    {
        _connStr = cfg.GetConnectionString("Sql")
                  ?? throw new InvalidOperationException("Sql conn-str?");
    }

    /* ---------- READ ------------------------------------------------ */
    public async Task<IReadOnlyList<TenantSchedule>> LoadAsync(CancellationToken ct = default)
    {
        const string sql = """
        /* 1️⃣  explicit schedules ----------------------------- */
        SELECT  s.TenantId, s.TenantName, s.RunTime, s.StartDate, s.EndDate,
                s.EnabledEndpoints, 1 AS HasSchedule
        FROM    dbo.TenantSchedule s

        UNION ALL

        /* 2️⃣  tenants discovered via Organisation ------------- */
        SELECT  t.TenantId, t.TenantName,
                CAST('01:00' AS time)            AS RunTime,
                CAST(GETUTCDATE() AS date)       AS StartDate,
                CAST(DATEADD(YEAR,1,GETUTCDATE()) AS date) AS EndDate,
                N'[]'                            AS EnabledEndpoints,
                0 AS HasSchedule
        FROM    ods.vwDistinctTenants t
        WHERE   NOT EXISTS (SELECT 1
                            FROM dbo.TenantSchedule s
                            WHERE s.TenantId = t.TenantId);
        """;

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand(sql, conn);
        await using var rdr = await cmd.ExecuteReaderAsync(ct);

        var list = new List<TenantSchedule>();

        while (await rdr.ReadAsync(ct))
        {
            var eps = JsonSerializer
                      .Deserialize<HashSet<string>>(rdr.GetString(5), _json)
                      ?? new();
            eps.Add("Organisation");           // mandatory

            list.Add(new TenantSchedule(
                TenantId: rdr.GetGuid(0),
                TenantName: rdr.IsDBNull(1) ? null : rdr.GetString(1),
                RunTime: TimeOnly.FromTimeSpan(rdr.GetTimeSpan(2)),
                StartDate: DateOnly.FromDateTime(rdr.GetDateTime(3)),
                EndDate: DateOnly.FromDateTime(rdr.GetDateTime(4)),
                EnabledEndpoints: eps));
        }

        return list;
    }



    /* ---------- WRITE (stub, Step 4) ------------------------------- */
    public Task SaveAsync(IReadOnlyList<TenantSchedule> model,
                          CancellationToken ct = default) =>
        throw new NotImplementedException(
            "Write support arrives in Step 4");
}
