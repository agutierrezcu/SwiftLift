using Ardalis.GuardClauses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Oakton;
using Serilog;
using SwiftLift.Infrastructure.Logging;

namespace SwiftLift.ServiceDefaults;

public static class AppRunnerExtensions
{
    public static async Task RunAppAsync(this WebApplication app, string[] args,
        ILogger logger)
    {
        Guard.Against.Null(app);
        Guard.Against.Null(args);

        app.AddApplicationLifetimeEvents(logger);

        await app.RunOaktonCommands(args)
            .ConfigureAwait(false);
    }

    private static void AddApplicationLifetimeEvents(this WebApplication app, ILogger logger)
    {
        Guard.Against.Null(app);
        Guard.Against.Null(logger);

        var lifetime = app.Lifetime;

        lifetime.ApplicationStarted.Register(
            () => logger.Information("{@EventId} Application started.", LoggingEvent.ApplicationStarted));

        lifetime.ApplicationStopping.Register(() => logger.Information("Application stopping."));
        lifetime.ApplicationStopped.Register(() => logger.Information("Application stopped."));
    }
}
