using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Core;

namespace Lucent.Auth
{
    public interface ILucentAuth
    {
        Task<string> GetAccessTokenAsync(CancellationToken ct);
    }

    public sealed class LucentAuth : ILucentAuth
    {
        private readonly IConfiguration _cfg;  // IConfiguration is already injected
        private readonly ILogger<LucentAuth> _log;

        private string? _cachedToken;
        private DateTime _expiresUtc;

        // Constructor is already correctly injecting IConfiguration and ILogger
        public LucentAuth(IConfiguration cfg, ILogger<LucentAuth> log)
        {
            _cfg = cfg;
            _log = log;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken ct)
        {
            // Reuse cached token if still valid for >5 minutes
            if (_cachedToken is not null && DateTime.UtcNow < _expiresUtc.AddMinutes(-5))
                return _cachedToken;

            // Prepare the request parameters using the injected IConfiguration
            var form = new Dictionary<string, string?>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = _cfg["Lucent:RefreshToken"],
                ["client_id"] = _cfg["Lucent:ClientId"],
                ["client_secret"] = _cfg["Lucent:ClientSecret"]
            };

            // Make the request to Xero's token endpoint
            var req = new RestRequest();
            foreach (var kv in form)
                req.AddParameter(kv.Key, kv.Value ?? string.Empty, ParameterType.GetOrPost);

            var resp = await new RestClient("https://identity.xero.com/connect/token")
                             .ExecutePostAsync(req, ct);

            // If the request fails, log the error and throw an exception
            if (!resp.IsSuccessful)
            {
                _log.LogError("Token refresh failed {Status}: {Body}",
                              (int)resp.StatusCode, resp.Content);
                throw new InvalidOperationException(
                    $"Token refresh failed: {(int)resp.StatusCode} {resp.Content}");
            }

            // Parse the response and store the token and its expiration date
            JsonNode json = JsonNode.Parse(resp.Content!)!;
            _cachedToken = json["access_token"]!.GetValue<string>();
            _expiresUtc = DateTime.UtcNow
                          .AddSeconds(json["expires_in"]!.GetValue<int>());
            string newRefresh = json["refresh_token"]!.GetValue<string>();

            _log.LogInformation("Access token refreshed, expires {Expiry}", _expiresUtc);

            // Persist the new refresh token to the configuration file using ConfigPathHelper
            var cfgPath = ConfigPathHelper.GetSharedConfigPath();  // Use helper for config path
            var root = JsonNode.Parse(await File.ReadAllTextAsync(cfgPath, ct))!;
            root["Lucent"]!["RefreshToken"] = newRefresh;
            await File.WriteAllTextAsync(
                cfgPath,
                root.ToJsonString(new() { WriteIndented = true }),
                ct);

            return _cachedToken;
        }
    }
}
