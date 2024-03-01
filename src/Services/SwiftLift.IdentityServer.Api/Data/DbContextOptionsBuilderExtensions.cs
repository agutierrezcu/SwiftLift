using System.Reflection;
using Ardalis.GuardClauses;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace SwiftLift.IdentityServer.Api.Data;

internal static class DbContextOptionsBuilderExtensions
{
    private const string MigrationsTableName = "__EFMigrationsHistory";

    internal static DbContextOptionsBuilder ConfigureDbContextOptions(
        this DbContextOptionsBuilder builder,
        Assembly assemblyMigrations,
        IdentityServerSchema schema,
        string? identityServerConnectionString = null)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(assemblyMigrations);

        builder
            .EnableDetailedErrors()
            .UseNpgsql(identityServerConnectionString ?? string.Empty,
                builder =>
                {
                    builder
                        .MigrationsAssembly(assemblyMigrations.GetName().Name)
                        .MigrationsHistoryTable(MigrationsTableName, schema.ToStringFast());
                })
            .UseExceptionProcessor();

        return builder;
    }
}
