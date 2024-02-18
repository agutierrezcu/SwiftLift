using Microsoft.Extensions.Logging;

namespace SwiftLift.Infrastructure.BuildInfo;

[ExcludeFromCodeCoverage]
internal partial class BuildInfoLogger(ILogger<BuildInfoLogger> _logger) : IBuildInfoLogger
{
    public void LogUnexpectedErrorLoadingBuildFile(Exception ex)
    {
        GeneratedLogUnexpectedErrorLoadingBuildFile(_logger, ex);
    }

    [LoggerMessage(
       Level = LogLevel.Error,
       Message = "Unexpected error reading or validating build info from file")]
    private static partial void GeneratedLogUnexpectedErrorLoadingBuildFile(ILogger logger, Exception ex);
}
