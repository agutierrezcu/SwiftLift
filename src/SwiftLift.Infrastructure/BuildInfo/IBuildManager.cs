namespace SwiftLift.Infrastructure.BuildInfo;

public interface IBuildManager
{
    Task<string> GetBuildAsJsonAsync(CancellationToken cancellation);

    Task<Build> GetBuildAsync(CancellationToken cancellation);
}
