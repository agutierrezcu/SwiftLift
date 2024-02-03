namespace SwiftLift.Infrastructure.Logging;

[ExcludeFromCodeCoverage]
internal sealed class AzureFileLoggingOptions
{
    public bool Enabled { get; init; } = true;

    public long FileSizeLimit { get; init; } = 4 * 1024 * 1024;

    public bool RollOnSizeLimit { get; init; } = true;

    public int RetainedFileCount { get; init; } = 1;

    public TimeSpan RetainTimeLimit { get; init; } = TimeSpan.FromDays(1);
}
