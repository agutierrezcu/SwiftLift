namespace SwiftLift.Infrastructure.BuildInfo;

public interface IFileReaderService
{
    Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken);
}
