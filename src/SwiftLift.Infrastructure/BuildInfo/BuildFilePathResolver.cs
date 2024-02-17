namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildFilePathResolver : IBuildFilePathResolver
{
    internal const string RelativePath = @"\build-info.json";

    public string GetRelativeToContentRoot()
    {
        return RelativePath;
    }
}
