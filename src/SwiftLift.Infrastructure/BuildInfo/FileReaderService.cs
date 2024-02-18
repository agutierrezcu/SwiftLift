namespace SwiftLift.Infrastructure.BuildInfo;

[ExcludeFromCodeCoverage]
public sealed class FileReaderService : IFileReaderService
{
    public Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(filePath);

        return File.ReadAllTextAsync(filePath, cancellationToken);
    }
}
