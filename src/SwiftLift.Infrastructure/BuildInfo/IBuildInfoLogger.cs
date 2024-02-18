namespace SwiftLift.Infrastructure.BuildInfo;

internal interface IBuildInfoLogger : IApplicationLogger
{
    void LogUnexpectedErrorLoadingBuildFile(Exception ex);
}
