using System.Reflection;
using Ardalis.GuardClauses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Oakton.Environment;
using Serilog;
using SwiftLift.Infrastructure.Logging;

namespace SwiftLift.ServiceDefaults;

public static partial class Extensions
{
    private static IServiceCollection AddValidators(this IServiceCollection services,
        Assembly[] assemblies)
    {
        Guard.Against.Null(services);
        Guard.Against.NullOrEmpty(assemblies);

        services.AddValidatorsFromAssemblies(assemblies,
            lifetime: ServiceLifetime.Singleton,
            includeInternalTypes: true);

        return services;
    }

    private static WebApplicationBuilder AddEnvironmentChecks(this WebApplicationBuilder builder,
        Assembly[] assemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrEmpty(assemblies);

        builder.Host.ApplyOaktonExtensions();

        var services = builder.Services;

        services
            .Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(s => s.AssignableTo<IEnvironmentCheck>(), false)
                .As<IEnvironmentCheck>()
                .WithTransientLifetime()
            );

        return builder;
    }

    public static async Task RunAppAsync(this WebApplication app, string[] args,
        ILogger logger)
    {
        Guard.Against.Null(app);
        Guard.Against.Null(logger);

        app.AddApplicationLifetimeEvents(logger);

        var checkResults = await EnvironmentChecker
            .ExecuteAllEnvironmentChecks(app.Services)
                .ConfigureAwait(false);

        if (!checkResults.Succeeded())
        {
            logger.Error("Environment Checks at {At} (Failed) {Results}",
                DateTime.Now.ToShortTimeString(), GetResultsSummary(checkResults));
        }

        checkResults.Assert();

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

    private static string GetResultsSummary(EnvironmentCheckResults checkResults)
    {
        Guard.Against.Null(checkResults);

        var sb = new StringWriter();

        sb.WriteLine();
        if (checkResults.Successes.Length > 0)
        {
            sb.WriteLine("=================================================================");
            sb.WriteLine("=                    Successful Checks                          =");
            sb.WriteLine("=================================================================");

            var successes = checkResults.Successes;
            foreach (var text in successes)
            {
                sb.WriteLine("* " + text);
            }

            sb.WriteLine();
        }

        if (checkResults.Failures.Length > 0)
        {
            sb.WriteLine("=================================================================");
            sb.WriteLine("=                          Failures                             =");
            sb.WriteLine("=================================================================");

            var failures = checkResults.Failures;
            foreach (var environmentFailure in failures)
            {
                sb.WriteLine("Failure: " + environmentFailure.Description);
                sb.WriteLine(environmentFailure.Exception.ToString());
                sb.WriteLine();
                sb.WriteLine();
            }
        }

        return sb.ToString();
    }
}
