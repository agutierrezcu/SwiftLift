using System.Reflection;
using Ardalis.GuardClauses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Oakton.Environment;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.Build;
using SwiftLift.Infrastructure.Environment;
using SwiftLift.Infrastructure.Serialization;

namespace SwiftLift.ServiceDefaults;

public static partial class Extensions
{
    public static WebApplicationBuilder AddSharedServices(this WebApplicationBuilder builder,
        Assembly[] assemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrEmpty(assemblies);

        var services = builder.Services;

        services.AddMemoryCache();

        services.AddSingleton<IApplicationInsightResource>(_ => ApplicationInsightResource.Instance);
        services.AddSingleton<IEnvironmentService>(_ => EnvironmentService.Instance);

        services.AddBuildInfo();
        services.AddSnakeSerialization();
        services.AddValidators(assemblies);

        return builder;
    }
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

    public static WebApplicationBuilder AddEnvironmentChecks(this WebApplicationBuilder builder,
        Assembly[] assemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrEmpty(assemblies);

        builder.Host.ApplyOaktonExtensions();

        builder.Services
            .Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(s => s.AssignableTo<IEnvironmentCheck>(), false)
                .As<IEnvironmentCheck>()
                .WithTransientLifetime()
            );

        return builder;
    }

    public static async Task RunAppAsync(this WebApplication app, string[] args)
    {
        Guard.Against.Null(app);

        var environmentCheckResults = await EnvironmentChecker
            .ExecuteAllEnvironmentChecks(app.Services)
                .ConfigureAwait(false);

        if (!environmentCheckResults.Succeeded())
        {
            var failedChecksFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}.err";

            environmentCheckResults.WriteToFile(failedChecksFileName);
        }

        environmentCheckResults.Assert();

        await app.RunOaktonCommands(args)
            .ConfigureAwait(false);
    }
}
