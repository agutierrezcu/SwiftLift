namespace SwiftLift.Infrastructure.BuildInfo;

internal interface IBuildFilePathResolver
{
    string GetRelativeToContentRoot();
}
