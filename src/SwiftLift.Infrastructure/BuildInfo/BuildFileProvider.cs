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

        using var streamReader = new StreamReader(fileInfo.CreateReadStream());

        return await streamReader.ReadToEndAsync(cancellation)
            .ConfigureAwait(false);
    }
}
