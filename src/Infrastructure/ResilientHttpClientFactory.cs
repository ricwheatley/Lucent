// Lucent.Resilience/ResilientHttpClientFactory.cs
using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lucent.Infrastructure;

public static class ResilientHttpClientFactory
{
    /// <summary>
    /// Registers an <see cref="HttpClient"/> typed client with retry + timeout.
    /// Usage: services.AddResilientHttpClient<IXeroApiClient, XeroApiClient>("xero");
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient<TClientInterface, TClientImpl>(
        this IServiceCollection services,
        string name)
        where TClientInterface : class
        where TClientImpl : class, TClientInterface
    {
        return services
            .AddHttpClient<TClientInterface, TClientImpl>(name)
            .ConfigureHttpClient(c =>
            {
                c.Timeout = TimeSpan.FromSeconds(100); // Outer timeout
            })
            .AddPolicyHandler((sp, _) =>
            {
                var logger = sp.GetRequiredService<ILogger<TClientImpl>>();
                return RetryPolicies.CreateHttpRetry(logger)
                                    .AddTimeout();
            });
    }
}
