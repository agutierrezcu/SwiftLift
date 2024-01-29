namespace SwiftLift.Infrastructure.BuildInfo;

internal interface IBuildFileProvider
{
    Task<string> GetContentAsync(CancellationToken cancellation);
}
