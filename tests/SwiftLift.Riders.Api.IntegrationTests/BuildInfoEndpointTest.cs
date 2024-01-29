using System.Net.Mime;
using FluentValidation;
using SwiftLift.Infrastructure.BuildInfo;
using SwiftLift.Infrastructure.Serialization;

namespace SwiftLift.Riders.Api.IntegrationTests;

public class BuildEndpointIntegrationTests(RidersApiWebApplicationFactory factory)
    : IClassFixture<RidersApiWebApplicationFactory>
{
    [Fact]
    public async Task Given_Build_File_When_Content_Is_Valid_Then_Return_As_Json()
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

        var build = JsonTextSnakeSerialization.Instance
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
}
