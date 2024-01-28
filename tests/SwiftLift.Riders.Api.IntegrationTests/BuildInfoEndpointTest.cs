using FluentValidation;
using SwiftLift.Infrastructure.Build;
using SwiftLift.Infrastructure.Serialization;

namespace SwiftLift.Riders.Api.IntegrationTests;

public class BuildInfoEndpointIntegrationTests(RidersApiWebApplicationFactory factory)
    : IClassFixture<RidersApiWebApplicationFactory>
{
    [Fact]
    public async Task Given_Build_Info_File_When_Content_Is_Valid_Then_Return_As_Json()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/build-info")
            .ConfigureAwait(true);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        response.Content.Headers?.ContentType?.ToString()
            .Should().Be("application/json; charset=utf-8");

        var buildInfoContent = await response.Content.ReadAsStringAsync()
            .ConfigureAwait(true);

        buildInfoContent.Should().NotBeNullOrEmpty();

        var buildInfo = JsonTextSnakeSerialization.Instance
            .Deserialize<BuildInfo>(buildInfoContent);

        var buildInfoValidator = new BuildInfoValidator();

        await buildInfoValidator
            .Invoking(
                async validator => await validator!.ValidateAndThrowAsync(buildInfo)
                    .ConfigureAwait(true))
            .Should()
            .NotThrowAsync()
                .ConfigureAwait(true);

        buildInfo.Should().Be(factory.BuildInfo);
    }
}
