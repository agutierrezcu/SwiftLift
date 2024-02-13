using SwiftLift.Infrastructure.Environment;

namespace SwiftLift.Infrastructure.UnitTests.Environment;

public sealed class EnvironmentServiceTests
{
    private readonly EnvironmentService _sut = EnvironmentService.Instance;

    [Fact]
    public void Given_EnvironmentVariableName_When_GetVariableCalled_Then_ReturnValue()
    {
        // Arrange
        System.Environment.SetEnvironmentVariable("variable1", "value1");

        // Act
        var value = _sut.GetVariable("variable1");

        // Assert
        value.Should().Be("value1");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Given_InvalidEnvironmentVariableName_When_GetVariableCalled_Then_ThrowArgumentException(string? variableName)
    {
        // Arrange

        // Act
        var act = () => _sut.GetVariable(variableName!);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Given_NonExistingEnvironmentVariableName_When_GetVariableCalled_Then_ReturnsNull()
    {
        // Arrange

        // Act
        var value = _sut.GetVariable("none");

        // Assert
        value.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Given_InvalidEnvironmentVariableName_When_GetRequiredVariableCalled_Then_ThrowArgumentException(
       string? variableName)
    {
        // Arrange

        // Act
        Action act = () => _sut.GetRequiredVariable(variableName!);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Given_EnvironmentVariableName_When_GetRequiredVariableCalled_Then_ReturnValue()
    {
        // Arrange
        System.Environment.SetEnvironmentVariable("variable2", "value2");

        // Act
        var value = _sut.GetRequiredVariable("variable2");

        // Assert
        value.Should().Be("value2");
    }

    [Fact]
    public void Given_EnvironmentVariableName_When_GetRequiredVariableCalled_Then_ThrowInvalidOperationException()
    {
        // Arrange
        System.Environment.SetEnvironmentVariable("variable3", "");

        // Act
        Action act = () => _sut.GetRequiredVariable("variable3");

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage($"Environment variable 'variable3' is not defined or value is not set");
    }
}
