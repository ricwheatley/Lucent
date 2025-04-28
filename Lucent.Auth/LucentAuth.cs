using System.Text.Json.Nodes;
using Lucent.Auth.TokenCache;
using Lucent.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Lucent.Auth;

public interface ILucentAuth
{
    Task<string> GetAccessTokenAsync(CancellationToken ct);
}

public sealed class LucentAuth : ILucentAuth
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<LucentAuth> _log;
    private readonly TimeSpan _earlyExpiry;

    private string? _cachedToken;
    private DateTime _expiresUtc;

    public LucentAuth(
        IConfiguration cfg,
        ILogger<LucentAuth> log,
        IOptions<TokenCacheOptions> opts)        // <-- new options injection
    {
        _cfg = cfg;
        _log = log;
        _earlyExpiry = TimeSpan.FromSeconds(opts.Value.EarlyExpirySeconds);
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        // Re-use the cached token if it has more than EarlyExpirySeconds left
        if (_cachedToken is not null && DateTime.UtcNow < _expiresUtc - _earlyExpiry)
            return _cachedToken;

        // Assemble the token-refresh form from configuration
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

        // Parse and cache the response
        JsonNode json = JsonNode.Parse(resp.Content!)!;
        _cachedToken = json["access_token"]!.GetValue<string>();
        _expiresUtc = DateTime.UtcNow
                        .AddSeconds(json["expires_in"]!.GetValue<int>());

        string newRefresh = json["refresh_token"]!.GetValue<string>();
        _log.LogInformation("Access token refreshed, expires {Expiry}", _expiresUtc);

        // Persist the new refresh token
        var cfgPath = ConfigPathHelper.GetSharedConfigPath();
        var root = JsonNode.Parse(await File.ReadAllTextAsync(cfgPath, ct))!;
        root["Lucent"]!["RefreshToken"] = newRefresh;
        await File.WriteAllTextAsync(cfgPath,
                                     root.ToJsonString(new() { WriteIndented = true }),
                                     ct);

        return _cachedToken;
    }
}
