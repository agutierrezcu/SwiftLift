namespace SwiftLift.Infrastructure.Build;

internal interface IBuildInfoFileProvider
{
    Task<string> GetContentAsync(CancellationToken cancellation);
}
