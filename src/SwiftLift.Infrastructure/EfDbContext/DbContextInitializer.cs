using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SwiftLift.Infrastructure.EfDbContext;

public sealed class DbContextInitializer<TDbContext>
    (IHostEnvironment _hostEnvironment,
    IServiceScopeFactory _serviceScopeFactory,
    ILogger<DbContextInitializer<TDbContext>> _logger)
        : BackgroundService
            where TDbContext : DbContext
{
    private static readonly string? s_dbContextName = typeof(TDbContext).Name;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (_hostEnvironment.IsProduction())
        {
            _logger.LogInformation("Pending migrations are not executed at start-up in Production for {DbContext}", s_dbContextName);

            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await RunPendingMigrationsAsync(dbContext, cancellationToken)
            .ConfigureAwait(false);
    }

    private readonly ActivitySource _activitySource =
        new($"{s_dbContextName}{DbContextInitializerActivity.MigrationsSourceNameSuffix}");

    private async Task RunPendingMigrationsAsync(TDbContext dbContext, CancellationToken cancellationToken)
    {
        Guard.Against.Null(dbContext);

        try
        {
            using var activity = _activitySource.StartActivity("Initializing catalog database {DbContext}",
                ActivityKind.Client);

            activity?.AddTag(nameof(DbContext), s_dbContextName);

            var sw = Stopwatch.StartNew();

            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(dbContext.Database.MigrateAsync, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Database migration completed after {ElapsedMilliseconds}ms for DbContext {DbContext} ",
                sw.ElapsedMilliseconds, s_dbContextName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error trying to run migrations for {DbContext} context", s_dbContextName);

            throw;
        }
    }
}
