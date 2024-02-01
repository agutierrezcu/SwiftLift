using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.BuildInfo;
using SwiftLift.Infrastructure.Serialization;

namespace SwiftLift.Riders.Api.IntegrationTests;

public sealed class RidersApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string BuildTestFileRelativePath = "/build-info-test.json";

    private string _buildPath = string.Empty;

    public Build? BuildTest { get; private set; }

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
            ArrangeBuildEndpointTest(context, services);
        });
    }

    private void ArrangeBuildEndpointTest(WebHostBuilderContext context, IServiceCollection services)
    {
        var buildPath = context.HostingEnvironment.ContentRootPath + BuildFilePathResolver.RelativePath;
        if (File.Exists(buildPath))
        {
            _buildPath = buildPath;

            InitializeBuild(context, BuildFilePathResolver.RelativePath);
        }
        else
        {
            _buildPath = context.HostingEnvironment.ContentRootPath + BuildTestFileRelativePath;

            ReplaceBuildFilePathResolver(services);

            CreateBuildFileTest();
        }
    }

    private void InitializeBuild(WebHostBuilderContext context, string relativePath)
    {
        var fileInfo = context.HostingEnvironment.ContentRootFileProvider.GetFileInfo(relativePath);

        var contentFile = ReadAllContent(fileInfo);

        BuildTest = SnakeJsonSerialization.Instance.Deserialize<Build>(contentFile);
    }

    private static string ReadAllContent(IFileInfo fileInfo)
    {
        using var reader = new StreamReader(fileInfo.CreateReadStream());

        return reader.ReadToEnd();
    }

    private static void ReplaceBuildFilePathResolver(IServiceCollection services)
    {
        var buildFilePathResolverDescriptor = services.Single(
            d => d.ServiceType == typeof(IBuildFilePathResolver));

        services.Remove(buildFilePathResolverDescriptor);

        var buildFilePathResolverMock = Substitute.For<IBuildFilePathResolver>();

        buildFilePathResolverMock.GetRelativeToContentRoot()
            .Returns(BuildTestFileRelativePath);

        services.AddSingleton(_ => buildFilePathResolverMock);
    }

    private void CreateBuildFileTest()
    {
        BuildTest = BuildFaker.Instance.Generate();

        var buildAsJson = SnakeJsonSerialization.Instance.Serialize(BuildTest);

        using var sw = File.CreateText(_buildPath);

        sw.Write(buildAsJson);
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
        if (_buildPath.EndsWith(BuildTestFileRelativePath))
        {
            File.Delete(_buildPath);
        }

        return Task.CompletedTask;
    }
}

