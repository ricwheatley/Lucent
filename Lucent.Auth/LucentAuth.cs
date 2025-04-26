using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;


namespace Lucent.Auth;

public interface ILucentAuth
{
    Task<string> GetAccessTokenAsync(CancellationToken ct);
}



public sealed class LucentAuth : ILucentAuth
{

    static string GetSharedConfigPath() =>
    Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..",   // up from bin/
        "config", "appsettings.json"));


    private readonly IConfiguration _cfg;
    private readonly ILogger<LucentAuth> _log;

    private string? _cachedToken;
    private DateTime _expiresUtc;

    public LucentAuth(IConfiguration cfg, ILogger<LucentAuth> log)
    {
        _cfg = cfg;
        _log = log;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        /* ── reuse cached token if still valid for >5 min ─────────────── */
        if (_cachedToken is not null && DateTime.UtcNow < _expiresUtc.AddMinutes(-5))
            return _cachedToken;

        /* ── build refresh request ────────────────────────────────────── */
        var form = new Dictionary<string, string?>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = _cfg["Lucent:RefreshToken"],
            ["client_id"] = _cfg["Lucent:ClientId"],
            ["client_secret"] = _cfg["Lucent:ClientSecret"]
            
        };

        var req = new RestRequest();
        foreach (var kv in form)

        
        req.AddParameter(kv.Key, kv.Value ?? string.Empty, ParameterType.GetOrPost);

        var resp = await new RestClient("https://identity.xero.com/connect/token")
                     .ExecutePostAsync(req, ct);

        if (!resp.IsSuccessful)
        {
            _log.LogError("Token refresh failed {Status}: {Body}",
                          (int)resp.StatusCode, resp.Content);
            throw new InvalidOperationException(
                $"Token refresh failed: {(int)resp.StatusCode} {resp.Content}");
        }

        /* ── parse response ───────────────────────────────────────────── */
        JsonNode json = JsonNode.Parse(resp.Content!)!;
        _cachedToken = json["access_token"]!.GetValue<string>();
        _expiresUtc = DateTime.UtcNow
                          .AddSeconds(json["expires_in"]!.GetValue<int>());
        string newRefresh = json["refresh_token"]!.GetValue<string>();

        _log.LogInformation("Access token refreshed, expires {Expiry}", _expiresUtc);

        /* ── persist new refresh_token so next run uses the latest one ─ */
        var cfgPath = GetSharedConfigPath();
        var root = JsonNode.Parse(await File.ReadAllTextAsync(cfgPath, ct))!;
        root["Lucent"]!["RefreshToken"] = newRefresh;
        await File.WriteAllTextAsync(
            cfgPath,
            root.ToJsonString(new() { WriteIndented = true }),
            ct);

        return _cachedToken;
    }
}
