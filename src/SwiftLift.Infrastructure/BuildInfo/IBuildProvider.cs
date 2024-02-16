namespace SwiftLift.Infrastructure.BuildInfo;

public interface IBuildProvider
{
    Task<string> GetBuildAsJsonAsync(CancellationToken cancellation);

    Task<Build> GetBuildAsync(CancellationToken cancellation);
}
