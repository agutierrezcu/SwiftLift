using Microsoft.Extensions.Logging;

namespace SwiftLift.Infrastructure.BuildInfo;

[ExcludeFromCodeCoverage]
internal sealed partial class BuildInfoLogger(ILogger<BuildInfoLogger> _logger) : IBuildInfoLogger
{
    public void LogUnexpectedExceptionLoadingBuildInfo(Exception ex)
    {
        GeneratedLogUnexpectedErrorLoadingBuildFile(_logger, ex);
    }

    [LoggerMessage(
       Level = LogLevel.Error,
       Message = "Unexpected error getting build info from file")]
    private static partial void GeneratedLogUnexpectedErrorLoadingBuildFile(ILogger logger, Exception ex);

    public void LogInvalidBuildInfo(string content, string errors)
    {
        GeneratedLogInvalidBuildInfo(_logger, content, errors);
    }

    [LoggerMessage(
      Level = LogLevel.Error,
      Message = "Build file contains invalid info. {Content} {Errors}")]
    private static partial void GeneratedLogInvalidBuildInfo(ILogger logger, string content, string errors);
}
