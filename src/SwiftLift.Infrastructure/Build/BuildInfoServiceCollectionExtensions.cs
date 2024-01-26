namespace SwiftLift.Infrastructure.Build;

public static class BuildInfoServiceCollectionExtensions
{
    public static IServiceCollection AddBuildInfo(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddSingleton<IBuildInfoFileProvider, BuildInfoFileProvider>();
        services.AddSingleton<IBuildInfoManager, BuildInfoManager>();

        return services;
    }
}
