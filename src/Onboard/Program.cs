using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using Lucent.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using System.Net.Http;
using Polly; // For resilience policies
using Polly.Extensions.Http;
using RestSharp;

namespace Lucent.Onboard
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create and configure a minimal host to provide logging services
            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddHttpClient("LucentHttp", client =>
                    {
                        // Use the default timeout defined in LucentDefaults
                        client.Timeout = LucentDefaults.HttpBaseDelay;
                    })
                    .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
                        .WaitAndRetryAsync(LucentDefaults.HttpRetryCount, retryAttempt => LucentDefaults.HttpBaseDelay));  // Use retry count and delay from LucentDefaults
                })
                .Build();

            // Resolve services
            var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Lucent.Onboard");

            logger.LogInformation("Lucent Onboard Initial Setup (Interactive)");

            // Load secrets from the encrypted local store
            var config = SecureConfigStore.Load();

            // Abort if essential config values are missing
            if (string.IsNullOrWhiteSpace(config.ClientId) ||
                string.IsNullOrWhiteSpace(config.ClientSecret) ||
                string.IsNullOrWhiteSpace(config.RedirectUri))
            {
                logger.LogError("Missing required configuration. Please ensure token.dat contains ClientId, ClientSecret and RedirectUri.");
                return;
            }

            // Construct a secure random state token to verify the redirect
            var state = Guid.NewGuid().ToString("N");

            // Build the OAuth2 consent URL with the expected scopes
            var consentUrl =
                "https://login.xero.com/identity/connect/authorize" +
                "?response_type=code" +
                $"&client_id={config.ClientId}" +
                "&redirect_uri=" + Uri.EscapeDataString(config.RedirectUri) +
                "&scope=" + Uri.EscapeDataString("offline_access accounting.transactions accounting.settings accounting.reports.read") +
                "&state=" + state;

            logger.LogInformation("Opening browser for Xero consent...");
            Process.Start(new ProcessStartInfo { FileName = consentUrl, UseShellExecute = true });

            // Start a local HTTP listener to capture the auth redirect
            var listener = new HttpListener();
            listener.Prefixes.Add(config.RedirectUri.EndsWith("/") ? config.RedirectUri : config.RedirectUri + "/");
            listener.Start();

            // Wait for the browser to return with the auth code
            var ctx = await listener.GetContextAsync();
            var query = HttpUtility.ParseQueryString(ctx.Request.Url!.Query);

            // Validate the returned state token
            if (query["state"] != state)
            {
                logger.LogError("State mismatch. Aborting.");
                ctx.Response.StatusCode = 400;
                ctx.Response.Close();
                return;
            }

            // Extract the auth code from the query string
            var code = query["code"]!;
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "text/html; charset=utf-8";
            var html = """
                <!doctype html>
                <html><body style='font-family:sans-serif'>
                  <h3>Authorised – you may close this window.</h3>
                </body></html>
            """;
            await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(html));
            ctx.Response.Close();
            logger.LogInformation("Authorisation code received.");

            // Use the auth code to request access and refresh tokens
            var tokenReq = new RestRequest()
                .AddParameter("grant_type", "authorization_code")
                .AddParameter("code", code)
                .AddParameter("redirect_uri", config.RedirectUri)
                .AddParameter("client_id", config.ClientId)
                .AddParameter("client_secret", config.ClientSecret);

            // Use IHttpClientFactory with Polly resilience to make the token exchange request
            var client = httpClientFactory.CreateClient("LucentHttp");
            var tokenResp = await client.ExecuteAsync(tokenReq);

            if (!tokenResp.IsSuccessful)
            {
                logger.LogError("Token exchange failed {Status}: {Body}", (int)tokenResp.StatusCode, tokenResp.Content);
                return;
            }

            // Parse and store the refresh token securely
            JsonNode json = JsonNode.Parse(tokenResp.Content!)!;
            config.RefreshToken = json["refresh_token"]!.GetValue<string>();
            SecureConfigStore.Save(config);
            logger.LogInformation("Refresh token stored.");

            // Use the access token to retrieve the list of connected tenants
            var accessToken = json["access_token"]!.GetValue<string>();
            var tenantReq = new RestRequest()
                .AddHeader("Authorization", $"Bearer {accessToken}");

            var tenantResp = await client.ExecuteAsync(tenantReq);

            if (!tenantResp.IsSuccessful)
            {
                logger.LogError("Failed to fetch tenant(s): {Body}", tenantResp.Content);
                return;
            }

            // Log the list of authorised tenants with their details
            JsonNode? tenants = JsonNode.Parse(tenantResp.Content!);
            logger.LogInformation("Retrieved {Count} tenant(s):", tenants?.AsArray().Count);
            foreach (var tenant in tenants!.AsArray())
            {
                logger.LogInformation("{Id} — {Name} ({Type})",
                    tenant["tenantId"]?.GetValue<string>(),
                    tenant["tenantName"]?.GetValue<string>(),
                    tenant["tenantType"]?.GetValue<string>());
            }
        }
    }
}
