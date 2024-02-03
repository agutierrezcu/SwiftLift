namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildFileProvider(
    IHostEnvironment _hostEnvironment,
    IBuildFilePathResolver _buildFilePathResolver)
        : IBuildFileProvider
{
    public async Task<string> GetContentAsync(CancellationToken cancellation)
    {
        var relativePath = _buildFilePathResolver.GetRelativeToContentRoot();

        var fileInfo = _hostEnvironment.ContentRootFileProvider
            .GetFileInfo(relativePath);

        var fileContent = await ReadAllContentAsync(fileInfo, cancellation)
            .ConfigureAwait(false);

        return fileContent;
    }

    private static async Task<string> ReadAllContentAsync(IFileInfo fileInfo,
        CancellationToken cancellation)
    {
        Guard.Against.Null(fileInfo);

        using var reader = new StreamReader(fileInfo.CreateReadStream());

        return await reader.ReadToEndAsync(cancellation)
            .ConfigureAwait(false);
    }
}
