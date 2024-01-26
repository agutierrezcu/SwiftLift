namespace SwiftLift.Infrastructure.Build;

public interface IBuildInfoManager
{
    ValueTask<string> GetBuildInfoAsStringAsync(CancellationToken cancellation);
}
