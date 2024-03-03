using Microsoft.EntityFrameworkCore;

namespace SwiftLift.Infrastructure.EfDbContext;

public static class DbContextInitializerActivity<TDbContext>
    where TDbContext : DbContext
{
    public static string GetActivitySourceName() =>
        $"{typeof(TDbContext).Name}{"Initializer"}";
}
