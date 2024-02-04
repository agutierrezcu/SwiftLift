using Microsoft.AspNetCore.Hosting;

namespace SwiftLift.Infrastructure.Logging;

[ExcludeFromCodeCoverage]
internal sealed class AzureFileLoggingOptions
{
    internal static AzureFileLoggingOptions CreateDefault(IWebHostEnvironment environment)
    {
        Guard.Against.Null(environment);

        var pathTemplate =
            environment.IsDevelopment()
            ? "../../LogFiles/Application/{0}.txt"
            : "D:/home/LogFiles/Application/{0}.txt";

        return new()
        {
            Enabled = true,
            PathTemplate = pathTemplate,
            FileSizeLimit = 4 * 1024 * 1024,
            RollOnSizeLimit = true,
            RetainedFileCount = 1,
            RetainTimeLimit = TimeSpan.FromDays(1)
        };
    }

    public bool Enabled { get; init; }

    public string? PathTemplate { get; init; }

    public long FileSizeLimit { get; init; }

    public bool RollOnSizeLimit { get; init; }

    public int RetainedFileCount { get; init; }

    public TimeSpan RetainTimeLimit { get; init; }
}
