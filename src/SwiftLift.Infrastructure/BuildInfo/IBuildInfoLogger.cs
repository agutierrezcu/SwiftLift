namespace SwiftLift.Infrastructure.BuildInfo;

internal interface IBuildInfoLogger : IApplicationLogger
{
    void LogUnexpectedExceptionLoadingBuildInfo(Exception ex);

    void LogInvalidBuildInfo(string content, string errors);
}
