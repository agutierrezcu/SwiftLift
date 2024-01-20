using Microsoft.Extensions.Configuration;
using NSubstitute.ExceptionExtensions;
using SwiftLift.SharedKernel.ApplicationInsight;
using SwiftLift.SharedKernel.ConnectionString;
using SwiftLift.SharedKernel.Environment;

using static SwiftLift.SharedKernel.ApplicationInsight.ApplicationInsightResourceDefaults;

namespace SwiftLift.SharedKernel.UnitTests.ApplicationInsight;

public sealed class ApplicationInsightEnvironmentCheckTests
{
    private static readonly ApplicationInsightEnvironmentCheck s_sut = new();

    public class Given_Valid_ConnectionString
    {
        [Fact]
        public async Task When_Assert_Then_Not_Throw_Exception()
        {
            // Arrange
            var environmentServiceMock = Substitute.For<IEnvironmentService>();

            var configurationMock = Substitute.For<IConfiguration>();

            var connectionStringResource = ConnectionStringParser.Parse(
                ResourceName, "InstrumentationKey=00000000-0000-0000-0000-000000000000");

            var applicationInsightResourceMock = Substitute.For<IApplicationInsightResource>();

            applicationInsightResourceMock
                .GetConnectionStringGuaranteed(environmentServiceMock, configurationMock)
                .Returns(connectionStringResource);

            var serviceProviderMock = Substitute.For<IServiceProvider>();

            serviceProviderMock.GetService(typeof(IApplicationInsightResource))
                .Returns(applicationInsightResourceMock);

            serviceProviderMock.GetService(typeof(IEnvironmentService))
                .Returns(environmentServiceMock);

            serviceProviderMock.GetService(typeof(IConfiguration))
                .Returns(configurationMock);

            // Act
            await s_sut.Assert(serviceProviderMock, default)
                .ConfigureAwait(true);

            // Assert
            serviceProviderMock
                .Received(1)
                .GetService(typeof(IApplicationInsightResource));

            serviceProviderMock
                .Received(1)
                .GetService(typeof(IEnvironmentService));

            serviceProviderMock
                .Received(1)
                .GetService(typeof(IConfiguration));

            applicationInsightResourceMock
                .Received(1)
                .GetConnectionStringGuaranteed(environmentServiceMock, configurationMock);
        }
    }

    public class Given_Invalid_ConnectionString
    {
        [Fact]
        public async Task When_Does_Not_Exist_In_Configuration_Then_Throw_Exception()
        {
            // Arrange
            var environmentServiceMock = Substitute.For<IEnvironmentService>();

            var configurationMock = Substitute.For<IConfiguration>();

            var applicationInsightResourceMock = Substitute.For<IApplicationInsightResource>();

            var invalidConnectionStringException =
                new InvalidConnectionStringException(ResourceName, "Invalid Connection String exception error message.");

            applicationInsightResourceMock
                .GetConnectionStringGuaranteed(environmentServiceMock, configurationMock)
                .Throws(invalidConnectionStringException);

            var serviceProviderMock = Substitute.For<IServiceProvider>();

            serviceProviderMock.GetService(typeof(IApplicationInsightResource))
                .Returns(applicationInsightResourceMock);

            serviceProviderMock.GetService(typeof(IEnvironmentService))
                .Returns(environmentServiceMock);

            serviceProviderMock.GetService(typeof(IConfiguration))
                .Returns(configurationMock);

            // Act
            var act = async () =>
                await s_sut.Assert(serviceProviderMock, default)
                    .ConfigureAwait(true);

            // Assert
            var exceptionAssertions = await act.Should()
                .ThrowExactlyAsync<InvalidConnectionStringException>()
                .WithMessage("Invalid Connection String exception error message.")
                    .ConfigureAwait(true);

            exceptionAssertions
                .Which.ResourceName.Should().Be(ResourceName);

            serviceProviderMock
                .Received(1)
                .GetService(typeof(IApplicationInsightResource));

            serviceProviderMock
                .Received(1)
                .GetService(typeof(IEnvironmentService));

            serviceProviderMock
                .Received(1)
                .GetService(typeof(IConfiguration));

            applicationInsightResourceMock
                .Received(1)
                .GetConnectionStringGuaranteed(environmentServiceMock, configurationMock);
        }

        [Fact]
        public async Task When_Exists_In_Configuration_With_No_Instrumentation_Key_Segment_Then_Throw_Exception()
        {
            // Arrange
            var environmentServiceMock = Substitute.For<IEnvironmentService>();

            var configurationMock = Substitute.For<IConfiguration>();

            var applicationInsightResourceMock = Substitute.For<IApplicationInsightResource>();

            var connectionStringResource = ConnectionStringParser.Parse(
                ResourceName, "key=value");

            applicationInsightResourceMock
                .GetConnectionStringGuaranteed(environmentServiceMock, configurationMock)
                .Returns(connectionStringResource);

            var serviceProviderMock = Substitute.For<IServiceProvider>();

            serviceProviderMock.GetService(typeof(IApplicationInsightResource))
                .Returns(applicationInsightResourceMock);

            serviceProviderMock.GetService(typeof(IEnvironmentService))
                .Returns(environmentServiceMock);

            serviceProviderMock.GetService(typeof(IConfiguration))
                .Returns(configurationMock);

            // Act
            var act = async () =>
                await s_sut.Assert(serviceProviderMock, default)
                    .ConfigureAwait(true);

            // Assert
            var exceptionAssertions = await act.Should()
                .ThrowExactlyAsync<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string does not have required InstrumentationKey segment key.")
                    .ConfigureAwait(true);

            exceptionAssertions
                .Which.ResourceName.Should().Be(ResourceName);

            serviceProviderMock
                .Received(1)
                .GetService(typeof(IApplicationInsightResource));

            serviceProviderMock
                .Received(1)
                .GetService(typeof(IEnvironmentService));

            serviceProviderMock
                .Received(1)
                .GetService(typeof(IConfiguration));

            applicationInsightResourceMock
                .Received(1)
                .GetConnectionStringGuaranteed(environmentServiceMock, configurationMock);
        }
    }
}
