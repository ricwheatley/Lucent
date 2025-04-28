// Lucent.Tests/TokenCacheEarlyExpiryTests.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Lucent.Auth;                          // namespace where LucentAuth lives
using Lucent.Auth.TokenCache;               // TokenCacheOptions
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Microsoft.AspNetCore.Http;
using Moq;
using RestSharp;
using System.Threading;

public class TokenCacheEarlyExpiryTests
{
    private static ILucentAuth BuildAuth(int earlyExpirySeconds)
    {
        var services = new ServiceCollection();

        // 1. mock RestClient so no HTTP call is made
        var rcMock = new Mock<IRestClient>();
        rcMock.Setup(m => m.ExecuteAsync(
                    It.IsAny<RestRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    ResponseStatus = ResponseStatus.Completed,
                    Content = """
                    {
                        "access_token"  : "dummy",
                        "expires_in"    : 3600,
                        "refresh_token" : "dummy"
                    }
             """
                });
        
        services.AddSingleton<IRestClient>(rcMock.Object);

        // minimal in-memory config
        var cfgDict = new Dictionary<string, string?>
        {
            ["Lucent:RefreshToken"] = "dummy",
            ["Lucent:ClientId"] = "dummy",
            ["Lucent:ClientSecret"] = "dummy",
            ["TokenCache:EarlyExpirySeconds"] = earlyExpirySeconds.ToString()
        };
        IConfiguration cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(cfgDict!)
            .Build();

        services.AddSingleton(cfg);
        services.AddLogging();
        services.Configure<TokenCacheOptions>(cfg.GetSection("TokenCache"));
        services.AddSingleton<ILucentAuth, LucentAuth>();

        return services.BuildServiceProvider()
                       .GetRequiredService<ILucentAuth>();
    }

    [Fact]
    public async Task Early_expiry_value_is_respected()
    {
        // Arrange – cache with 30-second early-expiry
        var auth = BuildAuth(30);

        // first call fetches a token and sets expiry to now + 3600s
        var token1 = await auth.GetAccessTokenAsync(default);

        // fast-forward just under 40 seconds: we expect refresh because 40 > 30
        await Task.Delay(TimeSpan.FromSeconds(40));

        var token2 = await auth.GetAccessTokenAsync(default);

        // Assert – token should have been refreshed, so two tokens differ
        Assert.NotEqual(token1, token2);
    }
}
