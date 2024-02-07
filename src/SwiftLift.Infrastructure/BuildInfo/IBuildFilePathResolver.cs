namespace SwiftLift.Infrastructure.BuildInfo;

public interface IBuildFilePathResolver
{
    string GetRelativeToContentRoot();
}
