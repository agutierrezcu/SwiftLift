namespace SwiftLift.Infrastructure.Build;

internal sealed class BuildInfoFileProvider(
    IHostEnvironment hostEnvironment, IBuildInfoFilePathResolver buildInfoFilePathResolver)
        : IBuildInfoFileProvider
{
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment
        ?? throw new ArgumentNullException(nameof(hostEnvironment));

    private readonly IBuildInfoFilePathResolver _buildInfoFilePathResolver = buildInfoFilePathResolver
        ?? throw new ArgumentNullException(nameof(buildInfoFilePathResolver));

    public async Task<string> GetContentAsync(CancellationToken cancellation)
    {
        var relativePath = _buildInfoFilePathResolver.GetRelativeToContentRoot();

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
