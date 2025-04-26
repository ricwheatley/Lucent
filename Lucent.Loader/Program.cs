// Lucent.Loader / Program.cs  – FULL FILE
using System.Data;
using System.Globalization;
using System.Text.Json;
using Lucent.Auth;
using Lucent.Client;
using Lucent.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestSharp;

static string GetSharedConfigPath() =>
    Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..",
        "config", "appsettings.json"));

/* -----------------------------------------------------------------
   0. Build host so we can DI LucentAuth + XeroApiClient + logging
------------------------------------------------------------------ */
var builder = Host.CreateApplicationBuilder(args);
var cfgPath = GetSharedConfigPath();

builder.Configuration.AddJsonFile(cfgPath, optional: false, reloadOnChange: true);
builder.Services.AddLogging(c => c.AddConsole());
builder.Services.AddSingleton<ILucentAuth, LucentAuth>();
builder.Services.AddSingleton<ILucentClient, XeroApiClient>();

var host = builder.Build();
var sp = host.Services;
var cfg = sp.GetRequiredService<IConfiguration>();
var log = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Loader");
var auth = sp.GetRequiredService<ILucentAuth>();

/* -----------------------------------------------------------------
   1. Ultra-simple arg reader
------------------------------------------------------------------ */
string? GetArg(string name)
{
    int i = Array.FindIndex(args, a => a.Equals(name, StringComparison.OrdinalIgnoreCase));
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}

/* -----------------------------------------------------------------
   2. Decide whether we’re doing normal endpoint loads or a report
------------------------------------------------------------------ */
string? report = GetArg("--report");
string tenantId = GetArg("--tenant") ?? cfg["Lucent:TenantId"]
                   ?? throw new InvalidOperationException("TenantId missing (arg or config)");

if (report is null)
{
    // start existing BackgroundService logic
    await host.RunAsync();
    return;
}

/* -----------------------------------------------------------------
   3. Dispatch report types
------------------------------------------------------------------ */
switch (report)
{
    case "TrialBalance":
    case "BalanceSheet":
        await RunReportAsync(
            reportName: report,
            tenantId: tenantId,
            asAt: ParseDate("--asAt"),
            from: null,
            to: null,
            cfg: cfg,
            auth: auth,
            log: log);
        break;

    case "ProfitAndLoss":
        await RunReportAsync(
            reportName: report,
            tenantId: tenantId,
            asAt: null,
            from: ParseDate("--from"),
            to: ParseDate("--to"),
            cfg: cfg,
            auth: auth,
            log: log);
        break;

    default:
        log.LogError("Unknown --report {R}", report);
        break;
}

/* -----------------------------------------------------------------
   Utility : parse YYYY-MM-DD or throw
------------------------------------------------------------------ */
DateTime ParseDate(string flag)
{
    var txt = GetArg(flag)
              ?? throw new ArgumentException($"{flag} YYYY-MM-DD is required");
    if (!DateTime.TryParseExact(txt, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out var d))
        throw new FormatException($"{flag} value '{txt}' is not a valid date");
    return d;
}

/* -----------------------------------------------------------------
   4. Real implementation – fetch & stage one report
------------------------------------------------------------------ */
static async Task RunReportAsync(
    string reportName,
    string tenantId,
    DateTime? asAt,
    DateTime? from,
    DateTime? to,
    IConfiguration cfg,
    ILucentAuth auth,
    ILogger log)
{
    /* 4A – build request ---------------------------------------- */
    var req = new RestRequest($"https://api.xero.com/api.xro/2.0/Reports/{reportName}");

    if (asAt != null) req.AddQueryParameter("date", asAt.Value.ToString("yyyy-MM-dd"));
    if (from != null) req.AddQueryParameter("fromDate", from.Value.ToString("yyyy-MM-dd"));
    if (to != null) req.AddQueryParameter("toDate", to.Value.ToString("yyyy-MM-dd"));

    req.AddHeader("Authorization", $"Bearer {await auth.GetAccessTokenAsync(default)}");
    req.AddHeader("Xero-tenant-id", tenantId);
    req.AddHeader("Accept", "application/json");

    /* 4B – execute ---------------------------------------------- */
    var resp = await new RestClient().ExecuteGetAsync(req);
    if (!resp.IsSuccessful)
    {
        log.LogError("Report {R} failed {S}: {Body}",
                     reportName, (int)resp.StatusCode, resp.Content);
        return;
    }

    /* 4C – stage into landing.Reports ---------------------------- */
    var parmsJson = JsonSerializer.Serialize(new { asAt, from, to });
    var connStr = cfg.GetConnectionString("Sql")
                    ?? throw new InvalidOperationException("Connection string missing");

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
    log.LogInformation("{Report} saved.  Params={@P}", reportName, parmsJson);
}
