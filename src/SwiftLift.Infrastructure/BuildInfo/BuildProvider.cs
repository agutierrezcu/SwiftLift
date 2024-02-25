using SwiftLift.Infrastructure.Exceptions;
using SwiftLift.Infrastructure.Serialization;

using static System.Environment;

namespace SwiftLift.Infrastructure.BuildInfo;

using BuildData = (Build? build, string buildContentAsJson);

internal sealed class BuildProvider
    (IBuildFileProvider _buildFileProvider,
    ISnakeJsonDeserializer _jsonSnakeDeserializer,
    IValidator<Build> _validator,
    IBuildInfoLogger _logger)
        : IBuildProvider
{
    private Task<BuildData>? _loadBuildTask;

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

    private Task<BuildData> LoadBuildOnceAsync(CancellationToken cancellation)
    {
        return _loadBuildTask ??= LoadBuildAsync(cancellation);
    }

    private async Task<BuildData> LoadBuildAsync(CancellationToken cancellation)
    {
        var buildAsString = "";

        try
        {
            buildAsString = await _buildFileProvider.GetContentAsync(cancellation)
                .ConfigureAwait(false);

            var build = _jsonSnakeDeserializer.Deserialize<Build>(buildAsString)!;

            var validationResult =
                await _validator.ValidateAsync(build, cancellation)
                    .ConfigureAwait(false);

            if (validationResult.IsValid)
            {
                return (build, buildAsString);
            }

            var errors = validationResult
                .ToDictionary()
                .Select(e => $"{e.Key}: {NewLine} {string.Join(NewLine, e.Value)}");

            var errorMessages = string.Join(NewLine, errors);

            _logger.LogInvalidBuildInfo(buildAsString, errorMessages);

            var sb = new StringBuilder(buildAsString)
                .AppendLine()
                .AppendLine("Invalid Build Info")
                .AppendLine(errorMessages);

            buildAsString = sb.ToString();

            return (build, buildAsString);
        }
        catch (Exception ex)
        {
            _logger.LogUnexpectedExceptionLoadingBuildInfo(ex);

            var sb = new StringBuilder(buildAsString)
                .AppendLine()
                .AppendLine("Unexpected error loading Build Info")
                .AppendLine(ex.GetDetails());

            buildAsString = sb.ToString();

            return (null, buildAsString);
        }
    }
}
