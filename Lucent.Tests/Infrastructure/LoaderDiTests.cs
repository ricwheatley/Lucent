using Lucent.Core.Loaders;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lucent.Tests;          // or Lucent.Tests.Infrastructure – match your layout

public class LoaderDiTests
{
    [Fact]
    public void ILucentLoader_can_be_resolved_from_DI()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddScoped<ILucentLoader, NoOpLucentLoader>()
            .BuildServiceProvider();

        // Act
        var loader = services.GetRequiredService<ILucentLoader>();

        // Assert
        Assert.NotNull(loader);
    }
}
