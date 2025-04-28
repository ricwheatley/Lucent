using Xunit;
using Moq;
using FluentAssertions;
using Lucent.Core.Services;
using Lucent.Core.Repositories;
using Lucent.Core.Models;
using System;

namespace Lucent.Tests.UnitTests.Core;

public class ExampleServiceTests
{
    private readonly Mock<IExampleRepository> _mockRepository;
    private readonly ExampleService _exampleService;

    public ExampleServiceTests()
    {
        _mockRepository = new Mock<IExampleRepository>();
        _exampleService = new ExampleService(_mockRepository.Object);
    }

    [Fact]
    public void GetExampleById_ShouldReturnExample_WhenIdExists()
    {
        // Arrange
        var exampleId = 1;
        var example = new ExampleModel { Id = exampleId, Name = "Enterprise Grade Example" };

        _mockRepository.Setup(repo => repo.GetById(exampleId))
                       .Returns(example);

        // Act
        var result = _exampleService.GetExampleById(exampleId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(exampleId);
        result.Name.Should().Be("Enterprise Grade Example");
        _mockRepository.Verify(repo => repo.GetById(exampleId), Times.Once);
    }

    [Fact]
    public void GetExampleById_ShouldThrowException_WhenIdDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;
        _mockRepository.Setup(repo => repo.GetById(nonExistentId))
                       .Returns((ExampleModel)null);

        // Act
        var act = () => _exampleService.GetExampleById(nonExistentId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"No Example found with Id {nonExistentId}");

        _mockRepository.Verify(repo => repo.GetById(nonExistentId), Times.Once);
    }
}
