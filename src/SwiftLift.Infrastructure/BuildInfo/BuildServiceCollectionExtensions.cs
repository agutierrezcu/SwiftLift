namespace SwiftLift.Infrastructure.BuildInfo;

public static class BuildServiceCollectionExtensions
{
    public static IServiceCollection AddBuildInfo(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.TryAddSingleton<IBuildFilePathResolver, BuildFilePathResolver>();
        services.AddSingleton<IBuildFileProvider, BuildFileProvider>();
        services.TryAddSingleton<IBuildProvider, BuildProvider>();

        return services;
    }
}
