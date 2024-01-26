namespace SwiftLift.SharedKernel.Build;

public static class BuildInfoServiceCollectionExtensions
{
    public static IServiceCollection AddBuildInfo(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddSingleton<IBuildInfoFileProvider, BuildInfoFileProvider>();

        return services;
    }
}
