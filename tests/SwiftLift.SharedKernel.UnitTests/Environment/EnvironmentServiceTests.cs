using SwiftLift.SharedKernel.Environment;

namespace SwiftLift.SharedKernel.UnitTests.Environment;

public sealed class EnvironmentServiceTests
{
    private readonly EnvironmentService _sut = EnvironmentService.Instance;

    [Fact]
    public void Given_Environment_Variable_When_Exist_Then_Return_Value_Is_OK()
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
    public void Given_Invalid_Environment_Variable_Then_Throw_Exception(string? variableName)
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
    public void Given_Environment_Variable_When_Does_Not_Exist_Then_Returns_Null()
    {
        // Arrange

        // Act
        var value = _sut.GetVariable("none");

        // Assert
        value.Should().BeNull();
    }
}
