using Microsoft.Extensions.Configuration;
using NSubstitute.ExceptionExtensions;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.Infrastructure.Environment;

using static SwiftLift.Infrastructure.ApplicationInsight.ApplicationInsightSettings;

namespace SwiftLift.Infrastructure.UnitTests.ApplicationInsight;

public sealed class ApplicationInsightEnvironmentCheckTests
{
    private static readonly ApplicationInsightEnvironmentCheck s_sut = new();

    public class Given_ValidConnectionString
    {
        [Fact]
        public async Task When_Assert_Then_NotThrowException()
        {
            // Arrange
            var environmentService = Substitute.For<IEnvironmentService>();

            var configuration = Substitute.For<IConfiguration>();

            var connectionStringResource = ConnectionStringParser.Parse(
                ResourceName, "InstrumentationKey=00000000-0000-0000-0000-000000000000");

            var applicationInsightResource = Substitute.For<IApplicationInsightResource>();

            applicationInsightResource
                .GetConnectionStringGuaranteed(environmentService, configuration)
                .Returns(connectionStringResource);

            var serviceProvider = Substitute.For<IServiceProvider>();

            serviceProvider.GetService(typeof(IApplicationInsightResource))
                .Returns(applicationInsightResource);

            serviceProvider.GetService(typeof(IEnvironmentService))
                .Returns(environmentService);

            serviceProvider.GetService(typeof(IConfiguration))
                .Returns(configuration);

            // Act
            await s_sut.Assert(serviceProvider, default)
                .ConfigureAwait(true);

            // Assert
            serviceProvider
                .Received(1)
                .GetService(typeof(IApplicationInsightResource));

            serviceProvider
                .Received(1)
                .GetService(typeof(IEnvironmentService));

            serviceProvider
                .Received(1)
                .GetService(typeof(IConfiguration));

            applicationInsightResource
                .Received(1)
                .GetConnectionStringGuaranteed(environmentService, configuration);
        }
    }

    public class Given_InvalidConnectionString
    {
        [Fact]
        public async Task When_DoesNotExistInConfiguration_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var environmentService = Substitute.For<IEnvironmentService>();

            var configuration = Substitute.For<IConfiguration>();

            var applicationInsightResource = Substitute.For<IApplicationInsightResource>();

            var invalidConnectionStringException =
                new InvalidConnectionStringException(ResourceName, "Invalid Connection String exception error message");

            applicationInsightResource
                .GetConnectionStringGuaranteed(environmentService, configuration)
                .Throws(invalidConnectionStringException);

            var serviceProvider = Substitute.For<IServiceProvider>();

            serviceProvider.GetService(typeof(IApplicationInsightResource))
                .Returns(applicationInsightResource);

            serviceProvider.GetService(typeof(IEnvironmentService))
                .Returns(environmentService);

            serviceProvider.GetService(typeof(IConfiguration))
                .Returns(configuration);

            // Act
            var act = async () =>
                await s_sut.Assert(serviceProvider, default)
                    .ConfigureAwait(true);

            // Assert
            var exceptionAssertions = await act.Should()
                .ThrowExactlyAsync<InvalidConnectionStringException>()
                .WithMessage("Invalid Connection String exception error message")
                    .ConfigureAwait(true);

            exceptionAssertions
                .Which.ResourceName.Should().Be(ResourceName);

            serviceProvider
                .Received(1)
                .GetService(typeof(IApplicationInsightResource));

            serviceProvider
                .Received(1)
                .GetService(typeof(IEnvironmentService));

            serviceProvider
                .Received(1)
                .GetService(typeof(IConfiguration));

            applicationInsightResource
                .Received(1)
                .GetConnectionStringGuaranteed(environmentService, configuration);
        }

        [Fact]
        public async Task When_ExistsInConfigurationWithNoInstrumentationKeySegment_Then_ThrowInvalidConnectionStringException()
        {
            // Arrange
            var environmentService = Substitute.For<IEnvironmentService>();

            var configuration = Substitute.For<IConfiguration>();

            var applicationInsightResource = Substitute.For<IApplicationInsightResource>();

            var connectionStringResource = ConnectionStringParser.Parse(
                ResourceName, "key=value");

            applicationInsightResource
                .GetConnectionStringGuaranteed(environmentService, configuration)
                .Returns(connectionStringResource);

            var serviceProvider = Substitute.For<IServiceProvider>();

            serviceProvider.GetService(typeof(IApplicationInsightResource))
                .Returns(applicationInsightResource);

            serviceProvider.GetService(typeof(IEnvironmentService))
                .Returns(environmentService);

            serviceProvider.GetService(typeof(IConfiguration))
                .Returns(configuration);

            // Act
            var act = async () =>
                await s_sut.Assert(serviceProvider, default)
                    .ConfigureAwait(true);

            // Assert
            var exceptionAssertions = await act.Should()
                .ThrowExactlyAsync<InvalidConnectionStringException>()
                .WithMessage("Application Insight connection string does not have required InstrumentationKey segment key")
                    .ConfigureAwait(true);

            exceptionAssertions
                .Which.ResourceName.Should().Be(ResourceName);

            serviceProvider
                .Received(1)
                .GetService(typeof(IApplicationInsightResource));

            serviceProvider
                .Received(1)
                .GetService(typeof(IEnvironmentService));

            serviceProvider
                .Received(1)
                .GetService(typeof(IConfiguration));

            applicationInsightResource
                .Received(1)
                .GetConnectionStringGuaranteed(environmentService, configuration);
        }
    }
}
