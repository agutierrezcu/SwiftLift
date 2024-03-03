using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oakton;
using SwiftLift.Infrastructure.Logging;

namespace SwiftLift.ServiceDefaults;

public static class AppRunnerExtensions
{
    public static async Task RunAppAsync(this WebApplication app, string[] args)
    {
        Guard.Against.Null(app);
        Guard.Against.Null(args);

        app.AddApplicationLifetimeEvents();

        await app.RunOaktonCommands(args)
            .ConfigureAwait(false);
    }

    private static void AddApplicationLifetimeEvents(this WebApplication app)
    {
        Guard.Against.Null(app);

        var lifetime = app.Lifetime;

        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

        var logger = loggerFactory.CreateLogger(nameof(AppRunnerExtensions));

        lifetime.ApplicationStarted.Register(
            () => logger.LogInformation(LoggingEvent.ApplicationStarted, "Application started"));

        lifetime.ApplicationStopping.Register(
            () => logger.LogInformation(LoggingEvent.ApplicationStopping, "Application stopping"));

        lifetime.ApplicationStopped.Register(
            () => logger.LogInformation(LoggingEvent.ApplicationStopped, "Application stopped"));
    }
}
