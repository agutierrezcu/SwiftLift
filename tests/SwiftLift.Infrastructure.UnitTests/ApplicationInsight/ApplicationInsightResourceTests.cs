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

    public class Given_Invalid_Connection_String
    {
        private static Action Act(IEnvironmentService environmentService, IConfiguration configuration)
        {
            return () => s_sut.GetConnectionStringGuaranteed(environmentService, configuration);
        }

        [Fact]
        public void When_Does_Not_Exist_In_Configuration_Then_Throw_Exception()
        {
            // Arrange
            var environmentServiceMock = Substitute.For<IEnvironmentService>();

            environmentServiceMock
                .GetVariable(EnvironmentVariable)
                .ReturnsNull();

            var configurationMock = Substitute.For<IConfiguration>();

            configurationMock[EnvironmentVariable] = null;

            configurationMock[ConfigurationSectionKey] = null;

            // Act
            var actAssertions = Act(environmentServiceMock, configurationMock).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string can not be null or empty.")
                .Which.ResourceName.Should().Be(ResourceName);

            environmentServiceMock
                .Received(1)
                .GetVariable(EnvironmentVariable);

            configurationMock
                .Received(1)[EnvironmentVariable] = null;

            configurationMock
                .Received(1)[ConfigurationSectionKey] = null;
        }

        [Fact]
        public void When_Exists_In_Environment_And_Is_Not_Parseable_Then_Throw_Exception()
        {
            // Arrange
            var environmentServiceMock = Substitute.For<IEnvironmentService>();

            environmentServiceMock
                .GetVariable(EnvironmentVariable)
                .Returns("any other string", []);

            var configurationMock = Substitute.For<IConfiguration>();

            // Act
            var actAssertions = Act(environmentServiceMock, configurationMock).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string is invalid.")
                .Which.InnerException.Should().BeOfType<InvalidConnectionStringException>()
                .Which.ResourceName.Should().Be(ResourceName);

            environmentServiceMock
                .Received(1)
                .GetVariable(EnvironmentVariable);

            configurationMock
               .DidNotReceive()[EnvironmentVariable] = null;

            configurationMock
                .DidNotReceive()[ConfigurationSectionKey] = null;
        }

        [Fact]
        public void When_Exists_In_ConfigurationSection_And_Is_Not_Parseable_Then_Throw_Exception()
        {
            // Arrange
            var environmentServiceMock = Substitute.For<IEnvironmentService>();

            environmentServiceMock
                .GetVariable(EnvironmentVariable)
                .ReturnsNull();

            var configurationMock = Substitute.For<IConfiguration>();

            configurationMock[EnvironmentVariable] = null;

            configurationMock[ConfigurationSectionKey] = "any other string";

            // Act
            var actAssertions = Act(environmentServiceMock, configurationMock).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string is invalid.")
                .Which.InnerException.Should().BeOfType<InvalidConnectionStringException>()
                .Which.ResourceName.Should().Be(ResourceName);

            environmentServiceMock
                .Received(1)
                .GetVariable(EnvironmentVariable);

            configurationMock
               .Received(1)[EnvironmentVariable] = null;

            configurationMock
                .Received(1)[ConfigurationSectionKey] = "any other string";
        }

        [Fact]
        public void When_Exists_In_Environment_And_Has_No_Instrumentation_Key_Segment_Then_Throw_Exception()
        {
            // Arrange
            var environmentServiceMock = Substitute.For<IEnvironmentService>();

            var configurationMock = Substitute.For<IConfiguration>();

            environmentServiceMock
                .GetVariable(EnvironmentVariable)
                .Returns("key1=value1;key2=value2;key3=value3", []);

            // Act
            var actAssertions = Act(environmentServiceMock, configurationMock).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string has no instrumentation key segment.")
                .Which.ResourceName.Should().Be(ResourceName);

            environmentServiceMock
                .Received(1)
                .GetVariable(EnvironmentVariable);
        }

        [Fact]
        public void When_Exists_In_ConfigurationSection_And_Has_No_Instrumentation_Key_Segment_Then_Throw_Exception()
        {
            // Arrange
            var environmentServiceMock = Substitute.For<IEnvironmentService>();

            environmentServiceMock
                .GetVariable(EnvironmentVariable)
                .ReturnsNull();

            var configurationMock = Substitute.For<IConfiguration>();

            configurationMock[EnvironmentVariable] = null;

            configurationMock[ConfigurationSectionKey] = "key1=value1;key2=value2;key3=value3";

            // Act
            var actAssertions = Act(environmentServiceMock, configurationMock).Should();

            // Assert
            actAssertions
                .ThrowExactly<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string has no instrumentation key segment.")
                .Which.ResourceName.Should().Be(ResourceName);

            environmentServiceMock
                .Received(1)
                .GetVariable(EnvironmentVariable);

            configurationMock
              .Received(1)[EnvironmentVariable] = null;

            configurationMock
                .Received(1)[ConfigurationSectionKey] = "key1=value1;key2=value2;key3=value3";
        }
    }

    public class Given_Valid_ConnectionString_In_Environment
    {
        private const string ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000";

        private readonly ConnectionStringResource _connectionStringResource;

        private readonly IEnvironmentService _environmentServiceMock;

        public Given_Valid_ConnectionString_In_Environment()
        {
            // Arrange
            _environmentServiceMock = Substitute.For<IEnvironmentService>();

            var configurationMock = Substitute.For<IConfiguration>();

            _environmentServiceMock
                .GetVariable(EnvironmentVariable)
                .Returns(ConnectionString, []);

            // Act
            _connectionStringResource = s_sut.GetConnectionStringGuaranteed(_environmentServiceMock, configurationMock);
        }

        [Fact]
        public void When_Get_Then_Return_Not_Null()
        {
            _connectionStringResource.Should().NotBeNull();
        }

        [Fact]
        public void When_Get_Then_Resource_Name_Is_OK()
        {
            _connectionStringResource.Name.Should().Be(ResourceName);
        }

        [Fact]
        public void When_Get_Then_Resource_Value_Is_OK()
        {
            _connectionStringResource.Value.Should().Be(ConnectionString);
        }

        [Fact]
        public void When_Get_Then_Environment_Variable_Provider_Is_Called_Once()
        {
            _environmentServiceMock
                .Received(1)
                .GetVariable(EnvironmentVariable);
        }
    }
}
