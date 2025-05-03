
using System.Text.Json.Nodes;
using Lucent.Infrastructure;
using Lucent.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Lucent.Infrastructure;

public interface ILucentAuth
{
    Task<string> GetAccessTokenAsync(CancellationToken ct);
}

public sealed class LucentAuth : ILucentAuth
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<LucentAuth> _log;
    private readonly TimeSpan _earlyExpiry;
    private readonly IRestClient _rest;

    private string? _cachedToken;
    private DateTime _expiresUtc;

    public LucentAuth(
        IConfiguration cfg,
        ILogger<LucentAuth> log,
        IOptions<TokenCacheOptions> opts,
        IRestClient rest)
    {
        _cfg = cfg;
        _log = log;
        _rest = rest;
        _earlyExpiry = TimeSpan.FromSeconds(
                           opts.Value.EarlyExpirySeconds > 0
                               ? opts.Value.EarlyExpirySeconds
                               : LucentDefaults.TokenEarlyExpirySeconds);
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        // return cached token if still outside early-expiry window
        if (_cachedToken is not null && DateTime.UtcNow < _expiresUtc - _earlyExpiry)
            return _cachedToken;

        /* ───── build token-refresh form ─────────────────────────── */
        var req = new RestRequest("", Method.Post)
                      .AddParameter("grant_type", "refresh_token")
                      .AddParameter("refresh_token", _cfg["Lucent:RefreshToken"])
                      .AddParameter("client_id", _cfg["Lucent:ClientId"])
                      .AddParameter("client_secret", _cfg["Lucent:ClientSecret"]);

        var resp = await _rest.ExecuteAsync(req, ct);

        if (!resp.IsSuccessful)
        {
            _log.LogError("Token refresh failed {Status}: {Body}",
                          (int)resp.StatusCode, resp.Content);
            throw new InvalidOperationException(
                $"Token refresh failed: {(int)resp.StatusCode} {resp.Content}");
        }

        /* ───── parse response & cache ───────────────────────────── */
        JsonNode json = JsonNode.Parse(resp.Content!)!;
        _cachedToken = json["access_token"]!.GetValue<string>();
        _expiresUtc = DateTime.UtcNow.AddSeconds(json["expires_in"]!.GetValue<int>());
        string newRefresh = json["refresh_token"]!.GetValue<string>();

        _log.LogInformation("Access token refreshed; expires {Expiry:c} from now", _expiresUtc - DateTime.UtcNow);

        /* ───── optional: persist new refresh token ────────────────
           Here we just log – persisting securely depends on environment
           (Key Vault, AWS Secrets Manager, etc.). */
        if (newRefresh != _cfg["Lucent:RefreshToken"])
            _log.LogInformation("New refresh_token received – remember to update your secret store.");

        return _cachedToken;
    }
}
