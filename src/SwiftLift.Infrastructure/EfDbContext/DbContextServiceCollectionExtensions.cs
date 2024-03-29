using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;

namespace SwiftLift.Infrastructure.EfDbContext;

public static class DbContextServiceCollectionExtensions
{
    public static IServiceCollection AddHostedMigrationRunnerService<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        Guard.Against.Null(services);

        services.TryAddSingleton<DbContextInitializer<TDbContext>>();
        services.AddHostedService(sp => sp.GetRequiredService<DbContextInitializer<TDbContext>>());

        services.ConfigureOpenTelemetryTracerProvider(
            tracing =>
                tracing.AddSource(DbContextInitializer<TDbContext>.s_activitySourceName));

        return services;
    }
}
