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
        Assembly assemblyMigration,
        IdentityServerSchema schema,
        Action<NpgsqlEntityFrameworkCorePostgreSQLSettings>? configureSettings = null,
        Action<DbContextOptionsBuilder>? configureDbContextOptions = null)
            where TDbContext : DbContext
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrWhiteSpace(connectionName);
        Guard.Against.Null(assemblyMigration);

        var services = builder.Services;

        services.TryRemoveDbContextService<TDbContext>();

        builder.AddNpgsqlDbContext<TDbContext>(
            connectionName,
            configureSettings,
            opts =>
            {
                opts.ConfigureDbContextOptions(assemblyMigration, schema);

                configureDbContextOptions?.Invoke(opts);
            });

        if (builder.Environment.IsDevelopment())
        {
            services.AddHostedMigrationRunnerService<TDbContext>();
        }

        var dbContextName = typeof(TDbContext).Name;

        services.AddHealthChecks()
            .AddCheck<DbContextInitializerHealthCheck<TDbContext>>($"{dbContextName}Initializer");
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
