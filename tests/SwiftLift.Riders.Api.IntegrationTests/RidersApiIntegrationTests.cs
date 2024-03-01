using System.Net.Mime;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using SwiftLift.Infrastructure.Application;
using SwiftLift.Infrastructure.BuildInfo;
using SwiftLift.Infrastructure.Correlation;
using SwiftLift.Infrastructure.EventTypes;
using SwiftLift.Infrastructure.Serialization;
using SwiftLift.Infrastructure.UserContext;

namespace SwiftLift.Riders.Api.IntegrationTests;

public class RidersApiIntegrationTests(SwiftliftApiWebApplicationFactory<Program> factory)
    : IClassFixture<SwiftliftApiWebApplicationFactory<Program>>
{
    [Fact]
    public async Task Given_BuildFile_When_ContentIsValid_Then_ReturnAsJson()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/build-info")
            .ConfigureAwait(true);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        response.Content.Headers?.ContentType?.ToString()
            .Should().Be(MediaTypeNames.Application.Json);

        var buildContent = await response.Content.ReadAsStringAsync()
            .ConfigureAwait(true);

        buildContent.Should().NotBeNullOrEmpty();

        var build = SnakeJsonSerialization.Instance
            .Deserialize<Build>(buildContent);

        var buildValidator = new BuildValidator();

        await buildValidator
            .Invoking(
                async validator => await validator!.ValidateAndThrowAsync(build)
                    .ConfigureAwait(true))
            .Should()
            .NotThrowAsync()
                .ConfigureAwait(true);

        build.Should().Be(factory.BuildTest);
    }

    [Fact]
    public void Given_Services_When_ServiceProviderIsBuilt_Then_AllEnrichersShouldBeRegistered()
    {
        // Arrange
        var serviceProvider = factory.Services;

        // Act
        var enrichers = serviceProvider.GetServices<ILogEventEnricher>();

        // Assert
        enrichers.Should().NotBeNullOrEmpty();

        enrichers.Should().HaveCount(6);

        enrichers.Should().Contain(e => e is ApplicationInfoLogEventEnricher);
        enrichers.Should().Contain(e => e is BuildLogEventEnricher);
        enrichers.Should().Contain(e => e is CorrelationIdLogEventEnricher);
        enrichers.Should().Contain(e => e is EventIdNormalizeLogEventEnricher);
        enrichers.Should().Contain(e => e is EventTypeLogEventEnricher);
        enrichers.Should().Contain(e => e is RequestUserIdLogEventEnricher);
    }
}
