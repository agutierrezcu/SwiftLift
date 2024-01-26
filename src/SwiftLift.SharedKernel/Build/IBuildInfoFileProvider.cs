namespace SwiftLift.SharedKernel.Build;

public interface IBuildInfoFileProvider
{
    ValueTask<string> GetContentAsync();
}
