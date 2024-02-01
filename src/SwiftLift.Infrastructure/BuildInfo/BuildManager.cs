using Microsoft.Extensions.Logging;
using SwiftLift.Infrastructure.Exceptions;
using SwiftLift.Infrastructure.Serialization;

using static System.Environment;

namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildManager
    (IBuildFileProvider buildFileProvider,
    ISnakeJsonDeserializer jsonSnakeDeserializer,
    IValidator<Build> buildValidator,
    ILogger<BuildManager> logger)
        : IBuildManager
{
    private Task<(Build?, string)>? _loadBuildTask;

    public async Task<string> GetBuildAsJsonAsync(CancellationToken cancellation)
    {
        var (_, buildAsJson) = await LoadBuildOnceAsync(cancellation)
            .ConfigureAwait(false);

        return buildAsJson;
    }

    public async Task<Build> GetBuildAsync(CancellationToken cancellation)
    {
        var (build, _) = await LoadBuildOnceAsync(cancellation)
            .ConfigureAwait(false);

        return build!;
    }

    private Task<(Build?, string)> LoadBuildOnceAsync(CancellationToken cancellation)
    {
        return _loadBuildTask ??= LoadBuildAsync(cancellation);
    }

    private async Task<(Build?, string)> LoadBuildAsync(CancellationToken cancellation)
    {
        var buildAsString = "";

        try
        {
            buildAsString = await buildFileProvider.GetContentAsync(cancellation)
                .ConfigureAwait(false);

            var build = jsonSnakeDeserializer.Deserialize<Build>(buildAsString)!;

            var validationResult =
                await buildValidator.ValidateAsync(build, cancellation)
                    .ConfigureAwait(false);

            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                    .ToList();

                var sb = new StringBuilder(buildAsString)
                    .AppendLine()
                    .AppendLine(string.Join(NewLine, errorMessages));

                buildAsString = sb.ToString();
            }

            return (build, buildAsString);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error retrieving and validation build info from file.");

            var sb = new StringBuilder(buildAsString)
                .AppendLine()
                .AppendLine(ex.GetDetails());

            buildAsString = sb.ToString();

            return (null, buildAsString);
        }
    }
}
