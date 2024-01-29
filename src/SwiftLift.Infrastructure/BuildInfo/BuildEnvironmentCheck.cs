using SwiftLift.Infrastructure.Serialization;

namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildEnvironmentCheck : IEnvironmentCheck
{
    public string Description => $"Build info file {BuildFilePathResolver.RelativePath} must be valid and defined as part of CI.";

    public async Task Assert(IServiceProvider services, CancellationToken cancellation)
    {
        var buildFileProvider = services.GetRequiredService<IBuildFileProvider>();
        var jsonSnakeDeserializer = services.GetRequiredService<IJsonSnakeDeserializer>();
        var buildValidator = services.GetRequiredService<IValidator<Build>>();

        var fileContent = await buildFileProvider.GetContentAsync(cancellation)
            .ConfigureAwait(false);

        var build = jsonSnakeDeserializer.Deserialize<Build>(fileContent)!;

        await buildValidator.ValidateAndThrowAsync(build, cancellation)
            .ConfigureAwait(false);
    }
}
