using SwiftLift.Infrastructure.BuildInfo;

namespace SwiftLift.Infrastructure.UnitTests.BuildInfo;

public class BuildValidatorTests
{
    [Fact]
    public void Given_Build_When_InfoIsValid_Then_ValidationSuccessful()
    {
        // Arrange
        var build = new Build
        {
            Id = "123",
            Number = "1.0",
            Branch = "main",
            Commit = "abc123",
            Url = "https://example.com"
        };

        var sut = new BuildValidator();

        // Act
        var result = sut.Validate(build);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "1.0", "branch", "abc123", "https://example.com", nameof(Build.Id))]
    [InlineData("123", "", "branch", "abc123", "https://example.com", nameof(Build.Number))]
    [InlineData("123", "1.0", "", "abc123", "https://example.com", nameof(Build.Branch))]
    [InlineData("123", "1.0", "branch", "", "https://example.com", nameof(Build.Commit))]
    [InlineData("123", "1.0", "branch", "abc123", "", nameof(Build.Url))]
    [InlineData("123", "1.0", "branch", "abc123", "invalidurl", nameof(Build.Url))]
    public void Given_Build_When_InfoIsInvalid_Then_ValidateFails(
        string id, string number, string branch, string commit, string url, string propertyName)
    {
        // Arrange
        var build = new Build
        {
            Id = id,
            Number = number,
            Branch = branch,
            Commit = commit,
            Url = url
        };

        var sut = new BuildValidator();

        // Act
        var result = sut.Validate(build);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors.Should().ContainSingle();

        result.Errors[0].PropertyName.Should().Be(propertyName);
    }
}
