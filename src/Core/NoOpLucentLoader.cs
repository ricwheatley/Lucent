namespace Lucent.Core;

public sealed class NoOpLucentLoader : ILucentLoader
{
    public Task<LoadResult> RunAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new LoadResult(true));
}
