namespace SwiftLift.Infrastructure.Build;

internal interface IBuildInfoFilePathResolver
{
    string GetRelativeToContentRoot();
}
