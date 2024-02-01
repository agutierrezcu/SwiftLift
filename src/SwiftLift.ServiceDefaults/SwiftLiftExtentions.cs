using System.Reflection;
using Ardalis.GuardClauses;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace SwiftLift.ServiceDefaults;

public static partial class Extensions
{
    private static IServiceCollection AddValidators(this IServiceCollection services,
        Assembly[] assemblies)
    {
        Guard.Against.Null(services);
        Guard.Against.NullOrEmpty(assemblies);

        services.AddValidatorsFromAssemblies(assemblies,
            lifetime: ServiceLifetime.Singleton,
            includeInternalTypes: true);

        return services;
    }
}
