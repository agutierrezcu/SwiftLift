using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.BuildInfo;
using SwiftLift.Infrastructure.Serialization;
using SwiftLift.Riders.Api.IntegrationTests.BuildInfo;

namespace SwiftLift.Riders.Api.IntegrationTests;

public sealed class SwiftliftApiWebApplicationFactory<TEntryPoint>
    : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
        where TEntryPoint : class
{
    private const string BuildTestFileRelativePath = "/build-info-test.json";

    private string _buildPath = string.Empty;

    public Build? BuildTest { get; private set; }

    static SwiftliftApiWebApplicationFactory()
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

        var buildFilePathResolver = Substitute.For<IBuildFilePathResolver>();

        buildFilePathResolver.GetRelativeToContentRoot()
            .Returns(BuildTestFileRelativePath);

        services.AddSingleton(_ => buildFilePathResolver);
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

        Environment.SetEnvironmentVariable(
           "ASPNETCORE_ENVIRONMENT",
           "Production",
           EnvironmentVariableTarget.Process);

        return Task.CompletedTask;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        Environment.SetEnvironmentVariable(
            ApplicationInsightSettings.EnvironmentVariable,
            string.Empty,
            EnvironmentVariableTarget.Process);

        if (_buildPath.EndsWith(BuildTestFileRelativePath))
        {
            File.Delete(_buildPath);
        }

        return Task.CompletedTask;
    }
}

