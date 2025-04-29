// Lucent.Onboard / Program.cs  – secret-first refactor, zero JSON writes
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestSharp;

/* ─────────────────────────────────────────────────────────────
   0. Build minimal host for config & logging
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
}));

var host = builder.Build();
var cfg = host.Services.GetRequiredService<IConfiguration>();
var logger = host.Services.GetRequiredService<ILoggerFactory>()
                         .CreateLogger("Lucent.Onboard");

/* ─────────────────────────────────────────────────────────────
   1. Secrets & settings
   ──────────────────────────────────────────────────────────── */
var lucent = cfg.GetRequiredSection("Lucent");
string clientId = lucent["ClientId"] ?? throw new InvalidOperationException("ClientId missing");
string clientSecret = lucent["ClientSecret"] ?? throw new InvalidOperationException("ClientSecret missing");
string scopes = lucent["Scopes"] ?? "accounting.settings accounting.transactions";
int listenPort = int.TryParse(lucent["RedirectPort"], out var p) ? p : 5123;

/* ─────────────────────────────────────────────────────────────
   2. Build consent URL and open browser
   ──────────────────────────────────────────────────────────── */
var state = Guid.NewGuid().ToString("N");
var consentUrl =
    "https://login.xero.com/identity/connect/authorize" +
    "?response_type=code" +
    $"&client_id={clientId}" +
    "&redirect_uri=" + Uri.EscapeDataString($"http://localhost:{listenPort}/callback") +
    "&scope=" + Uri.EscapeDataString(scopes) +
    "&state=" + state;

logger.LogInformation("Opening browser for Xero consent…");
Process.Start(new ProcessStartInfo { FileName = consentUrl, UseShellExecute = true });

/* ─────────────────────────────────────────────────────────────
   3. Mini HTTP listener to capture redirect
   ──────────────────────────────────────────────────────────── */
using var listener = new HttpListener();
listener.Prefixes.Add($"http://localhost:{listenPort}/callback/");
listener.Start();

var ctx = await listener.GetContextAsync(); // waits for browser hit
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
const string html = """
    <!doctype html>
    <html><body style="font-family:sans-serif">
      <h3>Authorised – you may close this window.</h3>
    </body></html>
    """;
await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(html));
ctx.Response.Close();
logger.LogInformation("Authorisation code received.");

/* ─────────────────────────────────────────────────────────────
   4. Exchange code → tokens
   ──────────────────────────────────────────────────────────── */
var tokenReq = new RestRequest()
                  .AddParameter("grant_type", "authorization_code")
                  .AddParameter("code", code)
                  .AddParameter("redirect_uri", $"http://localhost:{listenPort}/callback")
                  .AddParameter("client_id", clientId)
                  .AddParameter("client_secret", clientSecret);

var tokenResp = await new RestClient("https://identity.xero.com/connect/token")
                     .ExecutePostAsync(tokenReq, CancellationToken.None);

if (!tokenResp.IsSuccessful)
{
    logger.LogError("Token exchange failed {Status}: {Body}",
                    (int)tokenResp.StatusCode, tokenResp.Content);
    return;
}

/* ─────────────────────────────────────────────────────────────
   5. Show refresh token + tenantId, prompt to save as secret
   ──────────────────────────────────────────────────────────── */
JsonNode json = JsonNode.Parse(tokenResp.Content!)!;
string refresh = json["refresh_token"]!.GetValue<string>();
string tenantId = json["tenants"]?[0]?["id"]?.GetValue<string>() ?? "UNKNOWN";

logger.LogInformation("────────────────────────────────────────────────────────");
logger.LogInformation("Refresh token (copy & store as a secret):");
logger.LogInformation(refresh);
logger.LogInformation("");
logger.LogInformation("TenantId: {Tenant}", tenantId);
logger.LogInformation("────────────────────────────────────────────────────────");
logger.LogInformation("Next step (local dev):");
logger.LogInformation("  dotnet user-secrets set Lucent:RefreshToken \"<paste>\"");
logger.LogInformation("  dotnet user-secrets set Lucent:TenantId    \"{0}\"", tenantId);
logger.LogInformation("");
logger.LogInformation("For CI/prod, add the same values to GitHub Actions secrets.");
logger.LogInformation("Done ✅");
