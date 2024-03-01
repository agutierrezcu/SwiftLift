namespace SwiftLift.Infrastructure.Validators;

public static class ValidatorsServiceCollectionExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services,
        params Assembly[] applicationAssemblies)
    {
        Guard.Against.Null(services);
        Guard.Against.NullOrEmpty(applicationAssemblies);

        services.AddValidatorsFromAssemblies(applicationAssemblies,
            lifetime: ServiceLifetime.Singleton,
            includeInternalTypes: true);

        return services;
    }
}
