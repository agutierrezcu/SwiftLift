using SwiftLift.SharedKernel.Serialization;

namespace SwiftLift.SharedKernel.Build;

internal sealed class BuildInfoFileProvider
    (IHostEnvironment hostEnvironment, IJsonSnakeDeserializer jsonSnakeDeserializer)
        : IBuildInfoFileProvider
{
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment
        ?? throw new ArgumentNullException(nameof(hostEnvironment));

    private readonly IJsonSnakeDeserializer _jsonSnakeDeserializer = jsonSnakeDeserializer
        ?? throw new ArgumentNullException(nameof(jsonSnakeDeserializer));

    private string? _buildInfoCache;

    public async ValueTask<string> GetContentAsync()
    {
        return _buildInfoCache ??=
            await ReadBuildInfoFile()
                .ConfigureAwait(false);
    }

    private async Task<string> ReadBuildInfoFile()
    {
        var fileInfo = _hostEnvironment.ContentRootFileProvider
            .GetFileInfo("/build-info.json");

        var fileContent = await ReadAllContent(fileInfo)
            .ConfigureAwait(false);

        var buildInfo = _jsonSnakeDeserializer.Deserialize<BuildInfo>(fileContent);

        if (buildInfo == null)
        {

        }

        return fileContent;
    }

    private static async Task<string> ReadAllContent(IFileInfo fileInfo)
    {
        Guard.Against.Null(fileInfo);

        using var reader = new StreamReader(fileInfo.CreateReadStream());

        return await reader.ReadToEndAsync()
            .ConfigureAwait(false);
    }
}
