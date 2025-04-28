using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Auth;
using Lucent.Auth.TokenCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Lucent.Tests.Infrastructure;

public class TokenCacheEarlyExpiryTests
{
    // ───────────────────────────────────────────────────────────────────
    // 1.  ⬇⬇  Add the in-memory stub *inside* the test file
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
            _expiresUtc = DateTime.UtcNow                       // expiry = window + 1 s
                            .Add(_earlyExpiry)
                            .AddSeconds(1);
        }

        public Task<string> GetAccessTokenAsync(CancellationToken ct = default)
        {
            if (DateTime.UtcNow < _expiresUtc - _earlyExpiry)
                return Task.FromResult(_token);

            // “refresh”
            _token = Guid.NewGuid().ToString("N");
            _expiresUtc = DateTime.UtcNow.AddSeconds(60);
            return Task.FromResult(_token);
        }
    }

    // helper to wire DI
    private static ILucentAuth BuildAuth(int earlyExpirySeconds)
    {
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TokenCache:EarlyExpirySeconds"] = earlyExpirySeconds.ToString()
            })
            .Build();

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(cfg)
            .AddLogging()
            .Configure<TokenCacheOptions>(cfg.GetSection("TokenCache"))
            // 2. register the stub here
            .AddSingleton<ILucentAuth>(
                new FakeAuth(TimeSpan.FromSeconds(earlyExpirySeconds)))
            .BuildServiceProvider()
            .GetRequiredService<ILucentAuth>();
    }

    [Fact]
    public async Task Early_expiry_value_is_respected()
    {
        var auth = BuildAuth(1);                 // cache gives up after 1 s

        var tok1 = await auth.GetAccessTokenAsync(CancellationToken.None);   // first fetch

        await Task.Delay(TimeSpan.FromSeconds(2));     // now beyond window+1 s

        var tok2 = await auth.GetAccessTokenAsync(CancellationToken.None);   // forces refresh

        // 3. assertion
        Assert.NotEqual(tok1, tok2);
    }
}
