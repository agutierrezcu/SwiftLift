using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SwiftLift.Infrastructure.EfDbContext;

public sealed class DbContextMigrationsRunner<TDbContext>
    (IHostEnvironment _hostEnvironment,
    IServiceScopeFactory _serviceScopeFactory,
    ILogger<DbContextMigrationsRunner<TDbContext>> _logger)
        : BackgroundService
            where TDbContext : DbContext
{

    private readonly ActivitySource _activitySource = new(DbContextMigrationsActivity.SourceName);

    private static readonly string? s_dbContextName = typeof(TDbContext).FullName;

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

    private async Task RunPendingMigrationsAsync(TDbContext dbContext, CancellationToken cancellationToken)
    {
        Guard.Against.Null(dbContext);

        using var activity = _activitySource.StartActivity("Initializing catalog database", ActivityKind.Client);

        activity?.AddTag(nameof(DbContext), s_dbContextName);

        var sw = Stopwatch.StartNew();

        await dbContext.Database.EnsureCreatedAsync(cancellationToken)
            .ConfigureAwait(false);

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(dbContext.Database.MigrateAsync, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Database migration completed after {ElapsedMilliseconds}ms for DbContext {DbContext} ",
            s_dbContextName, sw.ElapsedMilliseconds);
    }
}
