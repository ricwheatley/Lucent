using Lucent.Auth;
using Lucent.Auth.TokenCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using RestSharp;
using System.Collections.Generic;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Lucent.Tests.Infrastructure;

public class TokenCacheEarlyExpiryTests
{
    private static ILucentAuth BuildAuth(int earlyExpirySeconds)
    {
        // 1. basic in-memory config
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Lucent:RefreshToken"] = "dummy",
                ["Lucent:ClientId"] = "dummy",
                ["Lucent:ClientSecret"] = "dummy",
                ["TokenCache:EarlyExpirySeconds"] = earlyExpirySeconds.ToString()
            })
            .Build();

        // 2. canned JSON that expires in 35 seconds
        const string Json = """
        {
          "access_token"  : "tok1",
          "expires_in"    : 35,
          "refresh_token" : "r1"
        }
        """;

        // 3. mock IRestClient – intercept the ExecuteAsync overload used in LucentAuth
        var rcMock = new Mock<IRestClient>();

        rcMock.Setup(m => m.ExecuteAsync(
                           It.IsAny<RestRequest>(),
                           It.IsAny<CancellationToken>()))
              .ReturnsAsync(new RestResponse
              {
                  StatusCode = HttpStatusCode.OK,
                  ResponseStatus = ResponseStatus.Completed,
                  Content = Json
              });

        // 4. build DI container
        return new ServiceCollection()
            .AddSingleton<IConfiguration>(cfg)
            .AddLogging()
            .Configure<TokenCacheOptions>(cfg.GetSection("TokenCache"))
            .AddSingleton<IRestClient>(rcMock.Object)
            .AddSingleton<ILucentAuth, LucentAuth>()
            .BuildServiceProvider()
            .GetRequiredService<ILucentAuth>();
    }

    [Fact]
    public async Task Early_expiry_value_is_respected()
    {
        var auth = BuildAuth(30);             // 30-second early-expiry

        var t1 = await auth.GetAccessTokenAsync(default); // first fetch ("tok1")

        // wait just over the early-expiry threshold (35-30 = 5 s margin)
        await Task.Delay(TimeSpan.FromSeconds(6));

        var t2 = await auth.GetAccessTokenAsync(default); // forces refresh

        Assert.NotEqual(t1, t2);               // proved refresh occurred
    }
}
