using Microsoft.Extensions.Configuration;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.ConnectionString;

using static SwiftLift.Infrastructure.ApplicationInsight.ApplicationInsightSettings;

namespace SwiftLift.Infrastructure.UnitTests.ApplicationInsight;

public sealed class ApplicationInsightResourceTests
{
    private static readonly ApplicationInsightResource s_sut = ApplicationInsightResource.Instance;

    public class Given_InvalidConnectionString
    {
        private static Action Act(IConfiguration configuration)
        {
            return () => s_sut.GetConnectionStringGuaranteed(configuration);
        }

        [Fact]
        public void When_DoesNotExistInConfiguration_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();

            configuration[EnvironmentVariable] = null;

            configuration[ConfigurationSectionKey] = null;

            // Act
            var actAssertions = Act(configuration).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string can not be null or empty")
                .Which.ResourceName.Should().Be(ResourceName);

            configuration
                .Received(1)[EnvironmentVariable] = null;

            configuration
                .Received(1)[ConfigurationSectionKey] = null;
        }

        [Fact]
        public void When_ExistsInEnvironment_And_IsNotParseable_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();

            configuration[EnvironmentVariable] = "any other string";

            // Act
            var actAssertions = Act(configuration).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string is invalid")
                .Which.InnerException.Should().BeOfType<InvalidConnectionStringException>()
                .Which.ResourceName.Should().Be(ResourceName);

            configuration
               .Received(1)[EnvironmentVariable] = "any other string";

            configuration
                .DidNotReceive()[ConfigurationSectionKey] = null;
        }

        [Fact]
        public void When_ExistsInConfigurationSection_And_IsNotParseable_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();

            configuration[EnvironmentVariable] = null;

            configuration[ConfigurationSectionKey] = "any other string";

            // Act
            var actAssertions = Act(configuration).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string is invalid")
                .Which.InnerException.Should().BeOfType<InvalidConnectionStringException>()
                .Which.ResourceName.Should().Be(ResourceName);

            configuration
               .Received(1)[EnvironmentVariable] = null;

            configuration
                .Received(1)[ConfigurationSectionKey] = "any other string";
        }

        [Fact]
        public void When_ExistsInEnvironment_And_HasNoInstrumentationKeySegment_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();

            configuration[EnvironmentVariable] = "key1=value1;key2=value2;key3=value3";

            configuration[ConfigurationSectionKey] = null;

            // Act
            var actAssertions = Act(configuration).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string has no instrumentation key segment")
                .Which.ResourceName.Should().Be(ResourceName);

            configuration.Received(1)[EnvironmentVariable] = "key1=value1;key2=value2;key3=value3";
        }

        [Fact]
        public void When_ExistsInConfigurationSection_And_HasNoInstrumentationKeySegment_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();

            configuration[EnvironmentVariable] = null;

            configuration[ConfigurationSectionKey] = "key1=value1;key2=value2;key3=value3";

            // Act
            var actAssertions = Act(configuration).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string has no instrumentation key segment")
                .Which.ResourceName.Should().Be(ResourceName);

            configuration
              .Received(1)[EnvironmentVariable] = null;

            configuration
                .Received(1)[ConfigurationSectionKey] = "key1=value1;key2=value2;key3=value3";
        }
    }

    public class Given_ValidConnectionStringInEnvironment
    {
        private const string ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000";

        private readonly ConnectionStringResource _connectionStringResource;

        public Given_ValidConnectionStringInEnvironment()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();

            configuration[EnvironmentVariable] = ConnectionString;

            // Act
            _connectionStringResource = s_sut.GetConnectionStringGuaranteed(configuration);
        }

        [Fact]
        public void When_Get_Then_ReturnNotNull()
        {
            _connectionStringResource.Should().NotBeNull();
        }

        [Fact]
        public void When_Get_Then_ResourceNameIsOK()
        {
            _connectionStringResource.Name.Should().Be(ResourceName);
        }

        [Fact]
        public void When_Get_Then_ResourceValueIsOK()
        {
            _connectionStringResource.Value.Should().Be(ConnectionString);
        }
    }
}
