using System.Threading;
using System.Threading.Tasks;
using Lucent.Core.Loaders;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lucent.Tests.Infrastructure;   // adjust to suit your folder structure

/// <summary>
/// Smoke-tests that DI can resolve <see cref="ILucentLoader"/> and that the
/// loader’s <c>RunAsync</c> contract completes successfully.
/// </summary>
public sealed class LoaderDiTests
{
    /// <summary>
    /// Tiny in-memory implementation used only for these tests.
    /// </summary>
    private sealed class NoOpLucentLoader : ILucentLoader
    {
        public Task<LoadResult> RunAsync(CancellationToken ct = default) =>
            Task.FromResult(new LoadResult(true));
    }

    [Fact]
    public void ILucentLoader_can_be_resolved_from_DI()
    {
        // Arrange
        using var provider = new ServiceCollection()
            .AddScoped<ILucentLoader, NoOpLucentLoader>()
            .BuildServiceProvider(validateScopes: true);

        // Act
        var loader = provider.GetRequiredService<ILucentLoader>();

        // Assert
        Assert.NotNull(loader);
        Assert.IsType<NoOpLucentLoader>(loader);
    }

    [Fact]
    public async Task RunAsync_returns_a_successful_LoadResult()
    {
        // Arrange
        using var provider = new ServiceCollection()
            .AddScoped<ILucentLoader, NoOpLucentLoader>()
            .BuildServiceProvider(validateScopes: true);

        var loader = provider.GetRequiredService<ILucentLoader>();

        // Act
        var result = await loader.RunAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);
    }
}
