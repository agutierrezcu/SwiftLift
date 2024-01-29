namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildFileProvider(
    IHostEnvironment hostEnvironment, IBuildFilePathResolver buildFilePathResolver)
        : IBuildFileProvider
{
    public async Task<string> GetContentAsync(CancellationToken cancellation)
    {
        var relativePath = buildFilePathResolver.GetRelativeToContentRoot();

        var fileInfo = hostEnvironment.ContentRootFileProvider
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
