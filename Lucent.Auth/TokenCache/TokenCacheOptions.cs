// Lucent.Auth/TokenCache/TokenCacheOptions.cs
namespace Lucent.Auth.TokenCache;

public sealed class TokenCacheOptions
{
    /// <summary>
    /// Seconds before the real expiry we stop re-using the cached token.
    /// Defaults to 300 (5 min).
    /// </summary>
    public int EarlyExpirySeconds { get; init; } = 300;
}
