namespace SwiftLift.Infrastructure.Build;

[ExcludeFromCodeCoverage]
internal sealed class BuildInfoFilePathResolver : IBuildInfoFilePathResolver
{
    internal const string RelativePath = "/build-info.json";

    public string GetRelativeToContentRoot()
    {
        return RelativePath;
    }
}
