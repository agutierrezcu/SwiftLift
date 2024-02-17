namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildFileProvider(
    IBuildFilePathResolver _buildFilePathResolver,
    IHostEnvironment _hostEnvironment,
    IFileReaderService _fileReaderService)
        : IBuildFileProvider
{
    public Task<string> GetContentAsync(CancellationToken cancellation)
    {
        var relativePath = _buildFilePathResolver.GetRelativeToContentRoot();

        var fileInfo = _hostEnvironment.ContentRootFileProvider
            .GetFileInfo(relativePath);

        if (fileInfo is null or { Exists: false })
        {
            throw new InvalidOperationException($"File {relativePath} does not exists.");
        }

        return _fileReaderService.ReadAllTextAsync(fileInfo.PhysicalPath!, cancellation);
    }
}
