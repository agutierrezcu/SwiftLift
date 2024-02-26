using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SwiftLift.IdentityServer.Api.Models;

namespace SwiftLift.IdentityServer.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        var schema = IdentityServerSchema.Users.ToStringFast();

        builder.HasDefaultSchema(schema);

        base.OnModelCreating(builder);
    }
}
