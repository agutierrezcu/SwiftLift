using Microsoft.Extensions.DependencyInjection;
using SwiftLift.Infrastructure.BuildInfo;

namespace SwiftLift.Infrastructure.UnitTests.BuildInfo;

public class BuildServiceCollectionExtensionsTests
{
    private readonly ServiceCollection _services;

    public BuildServiceCollectionExtensionsTests()
    {
        // Arrange
        _services = new ServiceCollection();

        // Act
        _services.AddBuildInfo();
    }

    [Theory]
    [InlineData(typeof(IBuildFilePathResolver), typeof(BuildFilePathResolver))]
    [InlineData(typeof(IBuildFileProvider), typeof(BuildFileProvider))]
    [InlineData(typeof(IFileReaderService), typeof(FileReaderService))]
    [InlineData(typeof(IBuildProvider), typeof(BuildProvider))]
    [InlineData(typeof(IBuildInfoLogger), typeof(BuildInfoLogger))]
    public void Given_Services_When_AddBuildInfo_Then_ShouldRegisterExpectedTypesAsSingleton(
        Type serviceType, Type implementationType)
    {
        // Assert
        _services.Should().ContainSingle(d =>
            d.ServiceType == serviceType &&
            d.ImplementationType == implementationType &&
            d.Lifetime == ServiceLifetime.Singleton);
    }
}
