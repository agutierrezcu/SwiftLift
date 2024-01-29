using Serilog.Core;

namespace SwiftLift.Infrastructure.BuildInfo;

public static class BuildServiceCollectionExtensions
{
    public static IServiceCollection AddBuildInfo(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddSingleton<IBuildFilePathResolver, BuildFilePathResolver>();
        services.AddSingleton<IBuildFileProvider, BuildFileProvider>();
        services.AddSingleton<IBuildManager, BuildManager>();
        services.AddSingleton<ILogEventEnricher, BuildEventEnricher>();

        return services;
    }
}
