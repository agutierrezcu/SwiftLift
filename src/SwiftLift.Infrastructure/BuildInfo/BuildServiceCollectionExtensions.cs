namespace SwiftLift.Infrastructure.BuildInfo;

public static class BuildServiceCollectionExtensions
{
    public static IServiceCollection AddBuildInfo(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddSingleton<IBuildFileProvider, BuildFileProvider>();
        services.AddSingleton<IBuildInfoLogger, BuildInfoLogger>();
        services.TryAddSingleton<IBuildFilePathResolver, BuildFilePathResolver>();
        services.TryAddSingleton<IFileReaderService, FileReaderService>();
        services.TryAddSingleton<IBuildProvider, BuildProvider>();

        return services;
    }
}
