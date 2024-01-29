using System.Reflection;
using Ardalis.GuardClauses;
using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oakton;
using Oakton.Environment;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.Environment;
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

    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Guard.Against.Null(builder);

        builder.Logging
            .ClearProviders()
            // To use Azure Log Stream.
            .AddAzureWebAppDiagnostics();

        builder.UseSerilogLogger();

        var services = builder.Services;

        services.AddSingleton<IApplicationInsightResource>(_ => ApplicationInsightResource.Instance);
        services.AddSingleton<IEnvironmentService>(_ => EnvironmentService.Instance);

        // TODO: Make sure if the line below is needed to avoid collision between application insights and OPTL
        services.Configure<TelemetryConfiguration>(opts => opts.DisableTelemetry = true);

        return builder;
    }

    public static async Task RunAppAsync(this WebApplication app, string[] args,
        Serilog.ILogger logger)
    {
        Guard.Against.Null(app);
        Guard.Against.Null(logger);

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
