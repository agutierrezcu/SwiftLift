using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SwiftLift.IdentityServer.Api.Data;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder
            .UseNpgsql(
                opts =>
                {
                    var identityServerApiAssembly = typeof(Program).Assembly;
                    opts
                        .MigrationsAssembly(identityServerApiAssembly.GetName().Name)
                        .MigrationsHistoryTable("__EFMigrationsHistory", ApplicationDbContext.Schema);
                });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
