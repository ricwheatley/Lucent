// Lucent.Resilience/SqlRetryProvider.cs
using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;

namespace Lucent.Infrastructure;

/// <summary>
/// Wraps Dapper or raw ADO calls with the SQL retry policy.
/// </summary>
public sealed class SqlRetryProvider
{
    private readonly string _connectionString;
    private readonly ILogger _logger;
    private readonly IAsyncPolicy _sqlPolicy;

    public SqlRetryProvider(string connectionString, ILogger<SqlRetryProvider> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
        _sqlPolicy = RetryPolicies.CreateSqlRetry(_logger).AddTimeout();
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        Func<SqlConnection, Task<TResult>> action)
    {
        return await _sqlPolicy.ExecuteAsync(async () =>
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            return await action(conn);
        });
    }
}
