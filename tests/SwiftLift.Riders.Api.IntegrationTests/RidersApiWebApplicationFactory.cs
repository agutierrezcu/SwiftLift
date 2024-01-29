using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.Build;
using SwiftLift.Infrastructure.Serialization;

namespace SwiftLift.Riders.Api.IntegrationTests;

public sealed class RidersApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string BuildInfoTestFileRelativePath = "/build-info-test.json";

    private string _buildInfoPath = string.Empty;

    public BuildInfo? BuildInfoTest { get; private set; }

    static RidersApiWebApplicationFactory()
    {
        OaktonEnvironment.AutoStartHost = true;

        AutoFaker.Configure(builder =>
        {
            builder.WithConventions();
        });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            ArrangeBuildInfoEndpointTest(context, services);
        });
    }

    private void ArrangeBuildInfoEndpointTest(WebHostBuilderContext context, IServiceCollection services)
    {
        var buildInfoPath = context.HostingEnvironment.ContentRootPath + BuildInfoFilePathResolver.RelativePath;
        if (File.Exists(buildInfoPath))
        {
            _buildInfoPath = buildInfoPath;

            InitializeBuildInfo(context, BuildInfoFilePathResolver.RelativePath);
        }
        else
        {
            _buildInfoPath = context.HostingEnvironment.ContentRootPath + BuildInfoTestFileRelativePath;

            ReplaceBuildInfoFilePathResolver(services);

            CreateBuildInfoFileTest();
        }
    }

    private void InitializeBuildInfo(WebHostBuilderContext context, string relativePath)
    {
        var fileInfo = context.HostingEnvironment.ContentRootFileProvider.GetFileInfo(relativePath);

        var contentFile = ReadAllContent(fileInfo);

        BuildInfoTest = JsonTextSnakeSerialization.Instance.Deserialize<BuildInfo>(contentFile);
    }

    private static string ReadAllContent(IFileInfo fileInfo)
    {
        using var reader = new StreamReader(fileInfo.CreateReadStream());

        return reader.ReadToEnd();
    }

    private static void ReplaceBuildInfoFilePathResolver(IServiceCollection services)
    {
        var buildInfoFilePathResolverDescriptor = services.Single(
            d => d.ServiceType == typeof(IBuildInfoFilePathResolver));

        services.Remove(buildInfoFilePathResolverDescriptor);

        var buildInfoFilePathResolverMock = Substitute.For<IBuildInfoFilePathResolver>();

        buildInfoFilePathResolverMock.GetRelativeToContentRoot()
            .Returns(BuildInfoTestFileRelativePath);

        services.AddSingleton(_ => buildInfoFilePathResolverMock);
    }

    private void CreateBuildInfoFileTest()
    {
        BuildInfoTest = BuildInfoFaker.Instance.Generate();

        var buildInfoAsJson = JsonTextSnakeSerialization.Instance.Serialize(BuildInfoTest);

        using var sw = File.CreateText(_buildInfoPath);

        sw.Write(buildInfoAsJson);
    }

    public Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable(
            ApplicationInsightSettings.EnvironmentVariable,
            "InstrumentationKey=00000000-0000-0000-0000-000000000000",
            EnvironmentVariableTarget.Process);

        return Task.CompletedTask;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        if (_buildInfoPath.EndsWith(BuildInfoTestFileRelativePath))
        {
            File.Delete(_buildInfoPath);
        }

        return Task.CompletedTask;
    }
}

