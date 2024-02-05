using System.Reflection;
using Ardalis.GuardClauses;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace SwiftLift.ServiceDefaults;

public static partial class Extensions
{
    private static IServiceCollection AddValidators(this IServiceCollection services,
        Assembly[] applicationAssemblies)
    {
        Guard.Against.Null(services);
        Guard.Against.NullOrEmpty(applicationAssemblies);

        services.AddValidatorsFromAssemblies(applicationAssemblies,
            lifetime: ServiceLifetime.Singleton,
            includeInternalTypes: true);

        return services;
    }
}
