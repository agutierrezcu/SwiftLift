using System.Reflection;
using Ardalis.GuardClauses;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace SwiftLift.IdentityServer.Api.Data;

internal static class DbContextOptionsBuilderExtensions
{
    internal static DbContextOptionsBuilder ConfigureDbContextOptions(
        this DbContextOptionsBuilder builder,
        Assembly identityServerApiAssembly,
        IdentityServerSchema schema,
        string? identityServerConnectionString = null)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(identityServerApiAssembly);

        builder
            .EnableDetailedErrors()
            .UseNpgsql(identityServerConnectionString ?? string.Empty,
                builder =>
                {
                    builder
                        .MigrationsAssembly(identityServerApiAssembly.GetName().Name)
                        .MigrationsHistoryTable("__EFMigrationsHistory", schema.ToStringFast());
                })
            .UseExceptionProcessor();

        return builder;
    }
}
