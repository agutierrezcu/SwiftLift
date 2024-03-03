namespace SwiftLift.Infrastructure.Activity;

public static class ActivityServiceCollectionExtensions
{
    public static IServiceCollection AddActivitySourceProvider(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.TryAddSingleton(typeof(IActivitySourceProvider<>), typeof(ActivitySourceDefaultProvider<>));

        return services;
    }
}
