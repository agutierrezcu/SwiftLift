using Microsoft.Extensions.DependencyInjection;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.Build;
using SwiftLift.Infrastructure.Serialization;

namespace SwiftLift.Riders.Api.IntegrationTests;

public sealed class RidersApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string BuildInfoFileRelativePath = "/build-info-test.json";

    private static string s_buildInfoPath = string.Empty;

    public BuildInfo BuildInfo { get; } = BuildInfoFaker.Instance.Generate();

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
            ReplaceIBuildInfoFilePathResolver(services);

            CreateBuildInfoFileTest(context);
        });
    }

    private static void ReplaceIBuildInfoFilePathResolver(IServiceCollection services)
    {
        var buildInfoFilePathResolverDescriptor = services.Single(
            d => d.ServiceType == typeof(IBuildInfoFilePathResolver));

        services.Remove(buildInfoFilePathResolverDescriptor);

        var buildInfoFilePathResolverMock = Substitute.For<IBuildInfoFilePathResolver>();

        buildInfoFilePathResolverMock.GetRelativeToContentRoot()
            .Returns(BuildInfoFileRelativePath);

        services.AddSingleton(_ => buildInfoFilePathResolverMock);
    }

    private void CreateBuildInfoFileTest(WebHostBuilderContext context)
    {
        var buildInfoAsJson = JsonTextSnakeSerialization.Instance.Serialize(BuildInfo);

        s_buildInfoPath =
            context.HostingEnvironment.ContentRootPath + BuildInfoFileRelativePath;
        using var sw = File.CreateText(s_buildInfoPath);
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
        File.Delete(s_buildInfoPath);

        return Task.CompletedTask;
    }
}

