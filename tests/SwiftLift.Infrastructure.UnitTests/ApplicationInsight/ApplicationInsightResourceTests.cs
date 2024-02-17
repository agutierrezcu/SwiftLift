using Microsoft.Extensions.Configuration;
using NSubstitute.ReturnsExtensions;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.Infrastructure.Environment;

using static SwiftLift.Infrastructure.ApplicationInsight.ApplicationInsightSettings;

namespace SwiftLift.Infrastructure.UnitTests.ApplicationInsight;

public sealed class ApplicationInsightResourceTests
{
    private static readonly ApplicationInsightResource s_sut = ApplicationInsightResource.Instance;

    public class Given_InvalidConnectionString
    {
        private static Action Act(IEnvironmentService environmentService, IConfiguration configuration)
        {
            return () => s_sut.GetConnectionStringGuaranteed(environmentService, configuration);
        }

        [Fact]
        public void When_DoesNotExistInConfiguration_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var environmentService = Substitute.For<IEnvironmentService>();

            environmentService
                .GetVariable(EnvironmentVariable)
                .ReturnsNull();

            var configuration = Substitute.For<IConfiguration>();

            configuration[EnvironmentVariable] = null;

            configuration[ConfigurationSectionKey] = null;

            // Act
            var actAssertions = Act(environmentService, configuration).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string can not be null or empty")
                .Which.ResourceName.Should().Be(ResourceName);

            environmentService
                .Received(1)
                .GetVariable(EnvironmentVariable);

            configuration
                .Received(1)[EnvironmentVariable] = null;

            configuration
                .Received(1)[ConfigurationSectionKey] = null;
        }

        [Fact]
        public void When_ExistsInEnvironment_And_IsNotParseable_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var environmentService = Substitute.For<IEnvironmentService>();

            environmentService
                .GetVariable(EnvironmentVariable)
                .Returns("any other string", []);

            var configuration = Substitute.For<IConfiguration>();

            // Act
            var actAssertions = Act(environmentService, configuration).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string is invalid")
                .Which.InnerException.Should().BeOfType<InvalidConnectionStringException>()
                .Which.ResourceName.Should().Be(ResourceName);

            environmentService
                .Received(1)
                .GetVariable(EnvironmentVariable);

            configuration
               .DidNotReceive()[EnvironmentVariable] = null;

            configuration
                .DidNotReceive()[ConfigurationSectionKey] = null;
        }

        [Fact]
        public void When_ExistsInConfigurationSection_And_IsNotParseable_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var environmentService = Substitute.For<IEnvironmentService>();

            environmentService
                .GetVariable(EnvironmentVariable)
                .ReturnsNull();

            var configuration = Substitute.For<IConfiguration>();

            configuration[EnvironmentVariable] = null;

            configuration[ConfigurationSectionKey] = "any other string";

            // Act
            var actAssertions = Act(environmentService, configuration).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string is invalid")
                .Which.InnerException.Should().BeOfType<InvalidConnectionStringException>()
                .Which.ResourceName.Should().Be(ResourceName);

            environmentService
                .Received(1)
                .GetVariable(EnvironmentVariable);

            configuration
               .Received(1)[EnvironmentVariable] = null;

            configuration
                .Received(1)[ConfigurationSectionKey] = "any other string";
        }

        [Fact]
        public void When_ExistsInEnvironment_And_HasNoInstrumentationKeySegment_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var environmentService = Substitute.For<IEnvironmentService>();

            var configuration = Substitute.For<IConfiguration>();

            environmentService
                .GetVariable(EnvironmentVariable)
                .Returns("key1=value1;key2=value2;key3=value3", []);

            // Act
            var actAssertions = Act(environmentService, configuration).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string has no instrumentation key segment")
                .Which.ResourceName.Should().Be(ResourceName);

            environmentService
                .Received(1)
                .GetVariable(EnvironmentVariable);
        }

        [Fact]
        public void When_ExistsInConfigurationSection_And_HasNoInstrumentationKeySegment_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var environmentService = Substitute.For<IEnvironmentService>();

            environmentService
                .GetVariable(EnvironmentVariable)
                .ReturnsNull();

            var configuration = Substitute.For<IConfiguration>();

            configuration[EnvironmentVariable] = null;

            configuration[ConfigurationSectionKey] = "key1=value1;key2=value2;key3=value3";

            // Act
            var actAssertions = Act(environmentService, configuration).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string has no instrumentation key segment")
                .Which.ResourceName.Should().Be(ResourceName);

            environmentService
                .Received(1)
                .GetVariable(EnvironmentVariable);

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

        private readonly IEnvironmentService _environmentService;

        public Given_ValidConnectionStringInEnvironment()
        {
            // Arrange
            _environmentService = Substitute.For<IEnvironmentService>();

            var configuration = Substitute.For<IConfiguration>();

            _environmentService
                .GetVariable(EnvironmentVariable)
                .Returns(ConnectionString, []);

            // Act
            _connectionStringResource = s_sut.GetConnectionStringGuaranteed(_environmentService, configuration);
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

        [Fact]
        public void When_Get_Then_EnvironmentVariableProviderIsCalledOnce()
        {
            _environmentService
                .Received(1)
                .GetVariable(EnvironmentVariable);
        }
    }
}
