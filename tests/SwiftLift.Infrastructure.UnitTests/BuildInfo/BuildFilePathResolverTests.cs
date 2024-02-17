using SwiftLift.Infrastructure.BuildInfo;

namespace SwiftLift.Infrastructure.UnitTests.BuildInfo;

public class BuildFilePathResolverTests
{
    [Fact]
    public void Given_BuildFilePathResolver_When_GetRelativeToContentRoot_Then_ReturnExpectedValue()
    {
        // Arrange
        var sut = new BuildFilePathResolver();

        // Act
        var result = sut.GetRelativeToContentRoot();

        // Assert
        result.Should().Be(BuildFilePathResolver.RelativePath);
    }
}
