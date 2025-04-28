namespace Lucent.Core.Loaders;

/// <summary>
/// Abstraction over “kick off a load” so callers never call Process.Start directly.
/// </summary>
public interface ILucentLoader
{
    /// <summary>
    /// Triggers a load operation and returns a result when it completes or fails.
    /// </summary>
    Task<LoadResult> RunAsync(CancellationToken cancellationToken = default);
}

/// <summary>Simple result DTO – flesh out later if needed.</summary>
public record LoadResult(bool Success, string? Error = null);