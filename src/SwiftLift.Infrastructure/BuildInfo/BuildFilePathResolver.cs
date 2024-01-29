namespace SwiftLift.Infrastructure.BuildInfo;

[ExcludeFromCodeCoverage]
internal sealed class BuildFilePathResolver : IBuildFilePathResolver
{
    internal const string RelativePath = @"\build-info.json";

    public string GetRelativeToContentRoot()
    {
        return RelativePath;
    }
}
