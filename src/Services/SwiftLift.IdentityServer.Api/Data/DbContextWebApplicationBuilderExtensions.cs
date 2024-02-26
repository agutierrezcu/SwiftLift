using System.Reflection;
using Ardalis.GuardClauses;
using Aspire.Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using SwiftLift.Infrastructure.EfDbContext;

namespace SwiftLift.IdentityServer.Api.Data;

internal static class DbContextWebApplicationBuilderExtensions
{
    public static void AddIdentityServerDbContext<TDbContext>(this IHostApplicationBuilder builder,
        string connectionName,
        Assembly identityServerAssembly,
        IdentityServerSchema schema,
        Action<NpgsqlEntityFrameworkCorePostgreSQLSettings>? configureSettings = null,
        Action<DbContextOptionsBuilder>? configureDbContextOptions = null)
            where TDbContext : DbContext
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrWhiteSpace(connectionName);
        Guard.Against.Null(identityServerAssembly);

        var services = builder.Services;

        services.TryRemoveDbContextService<TDbContext>();

        builder.AddNpgsqlDbContext<TDbContext>(
            connectionName,
            configureSettings,
            builder =>
            {
                builder.ConfigureDbContextOptions(identityServerAssembly, schema);

                configureDbContextOptions?.Invoke(builder);
            });

        if (builder.Environment.IsDevelopment())
        {
            services.AddHostedMigrationRunnerService<TDbContext>();
        }

        var dbContextFullName = typeof(TDbContext).FullName;

        services.AddOpenTelemetry()
            .WithTracing(
                tracing => tracing.AddSource(
                    $"{dbContextFullName}{DbContextInitializerActivity.MigrationsSourceNameSuffix}"));

        services.AddHealthChecks()
            .AddCheck<DbContextInitializerHealthCheck<TDbContext>>($"{dbContextFullName}Initializer");
    }

    private static void TryRemoveDbContextService<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        Guard.Against.Null(services);

        var context = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(TDbContext));
        if (context is null)
        {
            return;
        }

        services.Remove(context);

        var option = services.FirstOrDefault(r => r.ServiceType == typeof(DbContextOptions<TDbContext>));
        if (option is null)
        {
            return;
        }

        services.Remove(option);
    }
}
