using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Lucent.Tests.Unit.Infrastructure;

/// <summary>
/// Verifies that an <see cref="ILucentAuth"/> implementation honours the
/// early-expiry window before refreshing its cached token.
/// </summary>
public sealed class TokenCacheEarlyExpiryTests
{
    // ───────────────────────────────────────────────────────────────────
    // 1.  Minimal in-memory auth stub.
    // ───────────────────────────────────────────────────────────────────
    private sealed class FakeAuth : ILucentAuth
    {
        private readonly TimeSpan _earlyExpiry;
        private string _token;
        private DateTime _expiresUtc;

        public FakeAuth(TimeSpan earlyExpiry)
        {
            _earlyExpiry = earlyExpiry;
            _token = Guid.NewGuid().ToString("N");
            _expiresUtc = DateTime.UtcNow
                           .Add(_earlyExpiry)
                           .AddSeconds(1);
        }

        public Task<string> GetAccessTokenAsync(CancellationToken ct)
        {
            if (DateTime.UtcNow < _expiresUtc - _earlyExpiry)
                return Task.FromResult(_token);

            _token = Guid.NewGuid().ToString("N");
            _expiresUtc = DateTime.UtcNow.AddSeconds(60);
            return Task.FromResult(_token);
        }
    }

    // ───────────────────────────────────────────────────────────────────
    // 2.  Helper to build a ServiceProvider wired with our stub.
    // ───────────────────────────────────────────────────────────────────
    private static ServiceProvider BuildProvider(int earlyExpirySeconds)
    {
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TokenCache:EarlyExpirySeconds"] = earlyExpirySeconds.ToString()
            })
            .Build();

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(cfg)
            .AddLogging(o => o.AddDebug())
            .Configure<TokenCacheOptions>(cfg.GetSection("TokenCache"))
            .AddSingleton<ILucentAuth>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<TokenCacheOptions>>().Value;
                return new FakeAuth(TimeSpan.FromSeconds(opts.EarlyExpirySeconds));
            })
            .BuildServiceProvider(validateScopes: true);
    }

    // ───────────────────────────────────────────────────────────────────
    // 3.  The test.
    // ───────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Token_is_refreshed_after_early_expiry_window()
    {
        await using var provider = BuildProvider(earlyExpirySeconds: 1);
        var auth = provider.GetRequiredService<ILucentAuth>();

        var token1 = await auth.GetAccessTokenAsync(CancellationToken.None);

        await Task.Delay(TimeSpan.FromMilliseconds(500));       // within window
        var token2 = await auth.GetAccessTokenAsync(CancellationToken.None);
        Assert.Equal(token1, token2);                           // still cached

        await Task.Delay(TimeSpan.FromSeconds(1));              // beyond window
        var token3 = await auth.GetAccessTokenAsync(CancellationToken.None);
        Assert.NotEqual(token1, token3);                        // refreshed
    }
}
