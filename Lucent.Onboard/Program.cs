// Program.cs  ── Lucent.Onboard
using System.Diagnostics;
using System.Net;
using System.Text.Json.Nodes;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using RestSharp;
using Microsoft.Extensions.DependencyInjection;


static string GetSharedConfigPath() =>
    Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..",   // up from bin/
        "config", "appsettings.json"));

var builder = Host.CreateApplicationBuilder(args);

/* 1 ─ load shared config ------------------------------------------------ */
var cfgPath = GetSharedConfigPath();   // identical logic in all three projects
builder.Configuration
       .AddJsonFile(cfgPath, optional: false, reloadOnChange: true);

/* 2 ─ logging ----------------------------------------------------------- */
builder.Services.AddLogging(c => c.AddConsole());

var host = builder.Build();
var cfg = host.Services.GetRequiredService<IConfiguration>();
var logger = host.Services.GetRequiredService<ILoggerFactory>()
                          .CreateLogger("Lucent.Onboard");

/* 3 ─ pull secrets & settings ------------------------------------------ */
var lucent = cfg.GetRequiredSection("Lucent");
string clientId = lucent["ClientId"] ?? throw new InvalidOperationException("ClientId missing");
string clientSecret = lucent["ClientSecret"] ?? throw new InvalidOperationException("ClientSecret missing");
string scopes = lucent["Scopes"] ?? "accounting.settings accounting.transactions";
int listenPort = int.TryParse(lucent["RedirectPort"], out var p) ? p : 5123;

/* 4 ─ build consent URL and open browser -------------------------------- */
var state = Guid.NewGuid().ToString("N");
string consentUrl =
    "https://login.xero.com/identity/connect/authorize" +
    "?response_type=code" +
    $"&client_id={clientId}" +
    "&redirect_uri=" + Uri.EscapeDataString($"http://localhost:{listenPort}/callback") +
    "&scope=" + Uri.EscapeDataString(scopes) +
    "&state=" + state;

logger.LogInformation("Opening browser for Xero consent…");
Process.Start(new ProcessStartInfo { FileName = consentUrl, UseShellExecute = true });

/* 5 ─ mini HTTP listener to capture redirect ---------------------------- */
using var listener = new HttpListener();
listener.Prefixes.Add($"http://localhost:{listenPort}/callback/");
listener.Start();

var ctx = await listener.GetContextAsync();        // waits for browser hit
var query = HttpUtility.ParseQueryString(ctx.Request.Url!.Query);

if (query["state"] != state)
{
    logger.LogError("State mismatch, aborting.");
    ctx.Response.StatusCode = 400;
    ctx.Response.Close();
    return;
}

string code = query["code"]!;
ctx.Response.StatusCode = 200;
ctx.Response.ContentType = "text/html; charset=utf-8";
const string html = "<!doctype html><html><body style=\"font-family:sans-serif\">" +
                    "<h3>Authorised – you may close this window.</h3></body></html>";
await ctx.Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(html));
ctx.Response.Close();
logger.LogInformation("Authorisation code received.");

/* 6 ─ exchange code → tokens ------------------------------------------- */
var form = new Dictionary<string, string?>
{
    ["grant_type"] = "authorization_code",
    ["code"] = code,
    ["redirect_uri"] = $"http://localhost:{listenPort}/callback",
    ["client_id"] = clientId,
    ["client_secret"] = clientSecret
};

var req = new RestRequest();
foreach (var kv in form)
    req.AddParameter(kv.Key, kv.Value ?? string.Empty, ParameterType.GetOrPost);

var tokenResp = await new RestClient("https://identity.xero.com/connect/token")
                 .ExecutePostAsync(req, CancellationToken.None);

if (!tokenResp.IsSuccessful)
{
    logger.LogError("Token exchange failed {Status}: {Body}",
                    (int)tokenResp.StatusCode, tokenResp.Content);
    return;
}

JsonNode json = JsonNode.Parse(tokenResp.Content!)!;
string refresh = json["refresh_token"]!.GetValue<string>();
string tenantId = json["tenants"]?[0]?["id"]?.GetValue<string>() ?? "REPLACE_ME";
logger.LogInformation("Refresh token obtained (first 30 chars): {Tok}…", refresh[..30]);

/* 7 ─ persist refresh token back into config --------------------------- */
JsonNode root = JsonNode.Parse(await File.ReadAllTextAsync(cfgPath))!;
root["Lucent"]!["RefreshToken"] = refresh;
root["Lucent"]!["TenantId"] = tenantId;

await File.WriteAllTextAsync(
    cfgPath, root.ToJsonString(new() { WriteIndented = true }));

logger.LogInformation("Saved refresh token to {File}.  Lucent.Loader is now ready.", cfgPath);
