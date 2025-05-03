using System.Threading;
using System.Threading.Tasks;
using Lucent.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lucent.Tests.Unit.Loader;

/// <summary>
/// Smoke-tests that DI can create an <see cref="ILucentLoader"/> and that
/// <c>RunAsync</c> completes successfully.
/// </summary>
public sealed class LoaderDiTests
{
    // Minimal stub used only here
    private sealed class NoOpLucentLoader : ILucentLoader
    {
        public Task<LoadResult> RunAsync(CancellationToken ct = default) =>
            Task.FromResult(new LoadResult(true));
    }

    private static ServiceProvider BuildProvider() =>
        new ServiceCollection()
            .AddScoped<ILucentLoader, NoOpLucentLoader>()
            .BuildServiceProvider(validateScopes: true);

    [Fact]
    public void ILucentLoader_can_be_resolved_from_DI()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var loader = scope.ServiceProvider.GetRequiredService<ILucentLoader>();

        Assert.NotNull(loader);
        Assert.IsType<NoOpLucentLoader>(loader);
    }

    [Fact]
    public async Task RunAsync_returns_a_successful_LoadResult()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var loader = scope.ServiceProvider.GetRequiredService<ILucentLoader>();
        var result = await loader.RunAsync();

        Assert.True(result.Success);
        Assert.Null(result.Error);
    }
}
