namespace SwiftLift.Infrastructure.Build;

internal sealed class BuildInfoFileProvider(IHostEnvironment hostEnvironment)
    : IBuildInfoFileProvider
{
    internal const string RelativePath = "/build-info.json";

    private readonly IHostEnvironment _hostEnvironment = hostEnvironment
        ?? throw new ArgumentNullException(nameof(hostEnvironment));

    public async Task<string> GetContentAsync(CancellationToken cancellation)
    {
        var fileInfo = _hostEnvironment.ContentRootFileProvider
            .GetFileInfo(RelativePath);

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
