// Lucent.Resilience/RetryPolicies.cs
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Registry;
using Polly.Timeout;

namespace Lucent.Resilience;

/// <summary>
/// Houses reusable Polly policies for HTTP and SQL operations.
/// Tweaked for Lucent defaults (3 attempts, exp back-off, 2-second jitter).
/// </summary>
public static class RetryPolicies
{
    /// <summary>Retry + exponential back-off for idempotent HTTP verbs.</summary>
   
    public const string StandardHttpPolicy = "standard-http";
    public static IAsyncPolicy<HttpResponseMessage> CreateHttpRetry(ILogger logger)
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(2), retryCount: 3, fastFirst: true);

        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => (int)r.StatusCode is >= 500 or 408)
            .WaitAndRetryAsync(delay,
                onRetry: (outcome, ts, attempt, ctx) =>
                    logger.LogWarning(outcome.Exception,
                        "HTTP retry {Attempt}/3 after {Delay}. Status={StatusCode}",
                        attempt, ts, outcome.Result?.StatusCode));
    }

    /// <summary>Retry a SQL operation that throws transient errors.</summary>
    public static IAsyncPolicy CreateSqlRetry(ILogger logger)
    {
        return Policy
            .Handle<Exception>(IsTransientSql)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(250), 5),
                (ex, ts, attempt, ctx) =>
                    logger.LogWarning(ex, "SQL retry {Attempt}/5 after {Delay}", attempt, ts));
    }

    /// <summary>Adds a 30-second hard timeout so hung tasks don’t stall runners.</summary>
    public static IAsyncPolicy AddTimeout(this IAsyncPolicy inner)
        => Policy.TimeoutAsync(TimeSpan.FromSeconds(30))
                 .WrapAsync(inner);

    public static IAsyncPolicy<TResult> AddTimeout<TResult>(
        this IAsyncPolicy<TResult> inner)
    => Policy.TimeoutAsync<TResult>(TimeSpan.FromSeconds(30))
             .WrapAsync(inner);

    private static bool IsTransientSql(Exception ex)
        => ex switch
        {
            // Microsoft.Data.SqlClient transient error numbers
            { Source: "Core .Net SqlClient Data Provider" } when ex.Message.Contains("error number 4060") => true,
            { Message: var m } when m.Contains("deadlocked") => true,
            _ => false
        };

    /// <summary>Builds a Polly registry pre-populated with Lucent’s standard policies.</summary>
    public static IPolicyRegistry<string> BuildRegistry(ILoggerFactory loggerFactory)
        => new PolicyRegistry
        {
            [StandardHttpPolicy] = CreateHttpRetry(loggerFactory.CreateLogger("HttpRetry"))
        };
}
