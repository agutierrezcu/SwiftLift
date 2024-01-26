using Microsoft.Extensions.Logging;
using SwiftLift.Infrastructure.Exceptions;
using SwiftLift.Infrastructure.Serialization;

using static System.Environment;

namespace SwiftLift.Infrastructure.Build;

internal sealed class BuildInfoManager
    (IBuildInfoFileProvider buildInfoFileProvider,
    IJsonSnakeDeserializer jsonSnakeDeserializer,
    IValidator<BuildInfo> buildInfoValidator,
    ILogger<BuildInfoManager> logger)
        : IBuildInfoManager
{
    private readonly IBuildInfoFileProvider _buildInfoFileProvider = buildInfoFileProvider
        ?? throw new ArgumentNullException(nameof(buildInfoFileProvider));

    private readonly IJsonSnakeDeserializer _jsonSnakeDeserializer = jsonSnakeDeserializer
        ?? throw new ArgumentNullException(nameof(jsonSnakeDeserializer));

    private readonly IValidator<BuildInfo> _buildInfoValidator = buildInfoValidator
        ?? throw new ArgumentNullException(nameof(buildInfoValidator));

    private readonly ILogger<BuildInfoManager> _logger = logger
        ?? throw new ArgumentNullException(nameof(_logger));

    private string? _buildInfoCache;

    public async ValueTask<string> GetBuildInfoAsStringAsync(CancellationToken cancellation)
    {
        return _buildInfoCache ??=
             await GetBuildInfoAsStringInternalAsync(cancellation)
                .ConfigureAwait(false);
    }

    private async Task<string> GetBuildInfoAsStringInternalAsync(CancellationToken cancellation)
    {
        var fileContent = "Build info is not initialized.";

        try
        {
            fileContent = await _buildInfoFileProvider.GetContentAsync(cancellation)
                .ConfigureAwait(false);

            var buildInfo = _jsonSnakeDeserializer.Deserialize<BuildInfo>(fileContent)!;

            var validationResult =
                await _buildInfoValidator.ValidateAsync(buildInfo, cancellation)
                    .ConfigureAwait(false);

            if (validationResult.IsValid)
            {
                return fileContent;
            }

            var errorMessages = validationResult.Errors
                .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                .ToList();

            var sb = new StringBuilder(fileContent)
                .AppendLine()
                .AppendLine(string.Join(NewLine, errorMessages));

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving and validation build info from file.");

            var sb = new StringBuilder(fileContent)
                .AppendLine()
                .AppendLine(ex.GetDetails());

            return sb.ToString();
        }
    }
}
