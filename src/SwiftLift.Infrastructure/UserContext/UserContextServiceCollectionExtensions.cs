namespace SwiftLift.Infrastructure.UserContext;

public static class UserContextServiceCollectionExtensions
{
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.TryAddSingleton<IUserContext, UserContext>();

        return services;
    }
}
