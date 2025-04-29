// Lucent.Loader / Program.cs
using System.Data;
using System.Globalization;
using System.Text.Json;
using Lucent.Auth;
using Lucent.Client;
using Lucent.Core;
using Lucent.Loader;
using Lucent.Resilience;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestSharp;

/* ─────────────────────────────────────────────────────────────
   0. Build host
   ──────────────────────────────────────────────────────────── */
var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
       .AddJsonFile("config/appsettings.Development.json", optional: true, reloadOnChange: true)
       .AddUserSecrets<Program>(optional: true)
       .AddEnvironmentVariables();

builder.Services.AddLogging(c => c.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
    o.IncludeScopes = true;
}));

builder.Services.AddScoped<ILucentAuth, LucentAuth>();
builder.Services.AddScoped<ILucentClient, XeroApiClient>();
builder.Services.AddHostedService<LucentWorker>();          // ← run nightly loads

/* ─── resilience (HTTP + SQL retries) ─────────────────────── */
builder.Services.AddResilientHttpClient<ILucentClient, XeroApiClient>("xero");

builder.Services.AddSingleton<SqlRetryProvider>(sp =>
{
    var cs = sp.GetRequiredService<IConfiguration>().GetConnectionString("Sql")
             ?? throw new InvalidOperationException("Connection string 'Sql' missing.");
    return new SqlRetryProvider(cs, sp.GetRequiredService<ILogger<SqlRetryProvider>>());
});
/* ───────────────────────────────────────────────────────────── */

var host = builder.Build();
var sp = host.Services;
var cfg = sp.GetRequiredService<IConfiguration>();
var log = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Loader");
var auth = sp.GetRequiredService<ILucentAuth>();
var client = sp.GetRequiredService<ILucentClient>();

/* ─────────────────────────────────────────────────────────────
   1. CLI helper
   ──────────────────────────────────────────────────────────── */
string? Arg(string flag)
{
    int i = Array.FindIndex(args, a => a.Equals(flag, StringComparison.OrdinalIgnoreCase));
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}

/* ─────────────────────────────────────────────────────────────
   2. Mode: background load vs one-off report
   ──────────────────────────────────────────────────────────── */
string? report = Arg("--report");
string? tenantId = Arg("--tenant") ?? cfg["Lucent:TenantId"];

if (report is null)
{
    // normal pipeline – LucentWorker does the work
    await host.RunAsync();
    return;
}

/* ───── resolve TenantId for ad-hoc report ────────────────── */
if (!Guid.TryParse(tenantId, out var tid))
{
    log.LogInformation("TenantId missing/invalid – discovering via /connections…");
    tid = await client.DiscoverFirstTenantIdAsync(CancellationToken.None);
    await EnsureTenantScheduledAsync(tid, log, cfg, CancellationToken.None);
}
tenantId = tid.ToString();

/* ─────────────────────────────────────────────────────────────
   3. Dispatch report types
   ──────────────────────────────────────────────────────────── */
switch (report)
{
    case "TrialBalance":
    case "BalanceSheet":
        await RunReportAsync(report, tenantId,
                             asAt: Parse("--asAt"), from: null, to: null);
        break;

    case "ProfitAndLoss":
        await RunReportAsync(report, tenantId,
                             asAt: null,
                             from: Parse("--from"),
                             to: Parse("--to"));
        break;

    default:
        log.LogError("Unknown --report {R}", report);
        break;
}

/* ─────────────────────────────────────────────────────────────
   Helpers
   ──────────────────────────────────────────────────────────── */
DateTime? Parse(string flag)
{
    var txt = Arg(flag);
    return txt == null
         ? null
         : DateTime.ParseExact(txt, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}

async Task EnsureTenantScheduledAsync(Guid tenantId, ILogger logger, IConfiguration cfg, CancellationToken ct)
{
    var connStr = cfg.GetConnectionString("Sql")
               ?? throw new InvalidOperationException("Connection string 'Sql' missing.");
    await using var conn = new SqlConnection(connStr);
    await conn.OpenAsync(ct);

    const string sql = @"
        IF NOT EXISTS (SELECT 1 FROM dbo.TenantSchedule WHERE TenantId = @tid)
        BEGIN
            INSERT INTO dbo.TenantSchedule
                (TenantId, TenantName, RunTime, StartDate, EndDate, EnabledEndpoints)
            VALUES
                (@tid, 'New tenant', '01:00', '2000-01-01', '2999-12-31', '[]');
        END";
    await new SqlCommand(sql, conn)
    { Parameters = { new("@tid", tenantId) } }
    .ExecuteNonQueryAsync(ct);

    logger.LogInformation("Tenant {Tenant} ensured in dbo.TenantSchedule", tenantId);
}

/* ─────────────────────────────────────────────────────────────
   4. Fetch & stage one report
   ──────────────────────────────────────────────────────────── */
async Task RunReportAsync(
    string reportName,
    string tenantId,
    DateTime? asAt,
    DateTime? from,
    DateTime? to)
{
    var req = new RestRequest($"https://api.xero.com/api.xro/2.0/Reports/{reportName}");

    if (asAt != null) req.AddQueryParameter("date", asAt.Value.ToString("yyyy-MM-dd"));
    if (from != null) req.AddQueryParameter("fromDate", from.Value.ToString("yyyy-MM-dd"));
    if (to != null) req.AddQueryParameter("toDate", to.Value.ToString("yyyy-MM-dd"));

    req.AddHeader("Authorization", $"Bearer {await auth.GetAccessTokenAsync(CancellationToken.None)}");
    req.AddHeader("Xero-tenant-id", tenantId);
    req.AddHeader("Accept", "application/json");

    var resp = await new RestClient().ExecuteGetAsync(req);
    if (!resp.IsSuccessful)
    {
        log.LogError("Report {R} failed {S}: {Body}",
                     reportName, (int)resp.StatusCode, resp.Content);
        return;
    }

    var parmsJson = JsonSerializer.Serialize(new { asAt, from, to });
    var connStr = cfg.GetConnectionString("Sql")
                   ?? throw new InvalidOperationException("Connection string missing.");

    await using var conn = new SqlConnection(connStr);
    await conn.OpenAsync();

    using var cmd = new SqlCommand(@"
        INSERT INTO landing.Reports (TenantId, ReportName, ParametersJson, RawJson)
        VALUES (@tid, @name, @parms, @raw);", conn);

    cmd.Parameters.Add("@tid", SqlDbType.UniqueIdentifier).Value = Guid.Parse(tenantId);
    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 60).Value = reportName;
    cmd.Parameters.Add("@parms", SqlDbType.NVarChar).Value = parmsJson;
    cmd.Parameters.Add("@raw", SqlDbType.NVarChar).Value = resp.Content!;

    await cmd.ExecuteNonQueryAsync();
    log.LogInformation("{Report} saved. Params={@P}", reportName, parmsJson);
}
