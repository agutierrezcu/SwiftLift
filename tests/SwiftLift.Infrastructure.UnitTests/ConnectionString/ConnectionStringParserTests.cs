using SwiftLift.Infrastructure.ConnectionString;
using static SwiftLift.Infrastructure.ConnectionString.ConnectionStringDefaults;

namespace SwiftLift.Infrastructure.UnitTests.ConnectionString;

public sealed class ConnectionStringParserTests
{
    private const string ResourceName = "resource name";

    [Theory]
    [InlineData(null, "connection", ";", "=", "resourceName")]
    [InlineData("resource", null, ";", "=", "connectionString")]
    [InlineData("resource", "connection", null, "=", "segmentSeparator")]
    [InlineData("resource", "connection", ";", null, "keywordValueSeparator")]
    public void Given_InvalidStringParameters(
        string? resource,
        string? connectionString,
        string? segmentSeparator,
        string? keywordValueSeparator,
        string parameterName)
    {
        // Act
        Action act = () =>
            ConnectionStringParser.Parse(
                resource,
                connectionString,
                segmentSeparator,
                keywordValueSeparator);

        var actAssertions = act.Should();

        // Assert
        actAssertions
            .ThrowExactly<ArgumentNullException>()
            .WithParameterName(parameterName)
            .WithMessage($"Value cannot be null. (Parameter '{parameterName}')");
    }

    public class Given_InvalidConnectionString
    {
        [Fact]
        public void When_StartWithSegmentSeparator_Then_ThrowInvalidConnectionStringException()
        {
            // Act
            Action act = () =>
               ConnectionStringParser.Parse(ResourceName, ";key1=value1;key2;value2");

            var actAssertions = act.Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage($"Connection string starts with segment separator '{SegmentSeparator}'")
                .Which.ResourceName.Should().Be(ResourceName);
        }

        [Fact]
        public void When_DuplicatedKeyword_Then_ThrowInvalidConnectionStringException()
        {
            // Act
            Action act = () =>
               ConnectionStringParser.Parse(ResourceName, "key1=value1;key1=value2");

            var actAssertions = act.Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Duplicated keyword 'key1'")
                .Which.ResourceName.Should().Be(ResourceName);
        }

        [Fact]
        public void When_ContainsTwoFollowingSegmentSeparator_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            Action act = () =>
               ConnectionStringParser.Parse(ResourceName, "key1=value1;;key2=value2");

            var actAssertions = act.Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage($"Connection string contains two following segment separators '{SegmentSeparator}'")
                .Which.ResourceName.Should().Be(ResourceName);
        }

        [Fact]
        public void When_AnyKeywordHasNoValue_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            Action act = () =>
               ConnectionStringParser.Parse(ResourceName, "key1=value1;key2");

            var actAssertions = act.Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage($"Connection string doesn't have value for keyword 'key2'")
                .Which.ResourceName.Should().Be(ResourceName);
        }

        [Fact]
        public void When_AnyKeywordHasEmptyValue_Then_ThrowInvalidConnectionStringException()
        {
            Action act = () =>
               ConnectionStringParser.Parse(ResourceName, "key1=;key2=value2");

            var actAssertions = act.Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage($"Connection string has keyword 'key1' with empty value")
                .Which.ResourceName.Should().Be(ResourceName);
        }

        [Fact]
        public void When_AnyValueHasNoKeyword_Then_ThrowInvalidConnectionStringException()
        {
            Action act = () =>
               ConnectionStringParser.Parse(ResourceName, "key1=value1;=value2");

            var actAssertions = act.Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage($"Connection string has value 'value2' with no keyword")
                .Which.ResourceName.Should().Be(ResourceName);
        }
    }

    public class Given_StandardValidConnectionString
    {
        private readonly ConnectionStringResource _connectionStringResource;

        public Given_StandardValidConnectionString()
        {
            // Act
            _connectionStringResource = ConnectionStringParser.Parse(
                ResourceName,
                "Driver=ODBDriver17forSQLServer;Server=myServerAddress;Database=myDataBase;UID=myUsername;PWD=myPassword");
        }

        [Theory]
        [InlineData("Driver", "ODBDriver17forSQLServer")]
        [InlineData("Server", "myServerAddress")]
        [InlineData("Database", "myDataBase")]
        [InlineData("UID", "myUsername")]
        [InlineData("PWD", "myPassword")]
        public void When_ParseWithDefaultSettings_Then_SegmentValueIsOK(string keyword, string expectedValue)
        {
            _connectionStringResource.TryGetSegmentValue(keyword, out var value);

            value.Should().Be(expectedValue);
        }
    }

    public class Given_CustomConnectionStringWithEmptySegmentValues
    {
        private readonly ConnectionStringResource _connectionStringResource;

        public Given_CustomConnectionStringWithEmptySegmentValues()
        {
            _connectionStringResource = ConnectionStringParser.Parse(
                ResourceName,
                "Driver<>ODB Driver 17 for SQL Server|Server<>|Database<>myDataBase|UID<>myUsername|PWD<>",
                segmentSeparator: "|",
                keywordValueSeparator: "<>",
                allowEmptyValues: true);
        }

        [Theory]
        [InlineData("Driver", "ODB Driver 17 for SQL Server")]
        [InlineData("Server", "")]
        [InlineData("Database", "myDataBase")]
        [InlineData("UID", "myUsername")]
        [InlineData("PWD", "")]
        public void When_ParseWithCustomSettings_Then_SegmentValueIsOK(string keyword, string expectedValue)
        {
            _connectionStringResource.TryGetSegmentValue(keyword, out var value);

            value.Should().Be(expectedValue);
        }
    }
}
