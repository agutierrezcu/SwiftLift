using SwiftLift.Infrastructure.Serialization;

namespace SwiftLift.Infrastructure.Build;

public class BuildInfoEnvironmentCheck : IEnvironmentCheck
{
    public string Description => $"Build info file {BuildInfoFilePathResolver.RelativePath} must be valid and defined as part of CI.";

    public async Task Assert(IServiceProvider services, CancellationToken cancellation)
    {
        var buildInfoFileProvider = services.GetRequiredService<IBuildInfoFileProvider>();
        var jsonSnakeDeserializer = services.GetRequiredService<IJsonSnakeDeserializer>();
        var buildInfoValidator = services.GetRequiredService<IValidator<BuildInfo>>();

        var fileContent = await buildInfoFileProvider.GetContentAsync(cancellation)
            .ConfigureAwait(false);

        var buildInfo = jsonSnakeDeserializer.Deserialize<BuildInfo>(fileContent)!;

        await buildInfoValidator.ValidateAndThrowAsync(buildInfo, cancellation)
            .ConfigureAwait(false);
    }
}
