using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Oakton.Environment;
using Oakton.Resources;
using SimpleInjector;
using SwiftLift.SharedKernel.ApplicationInsight;
using SwiftLift.SharedKernel.Build;
using SwiftLift.SharedKernel.Environment;
using SwiftLift.SharedKernel.Serialization;

namespace SwiftLift.ServiceDefaults;

public static partial class Extensions
{
    public static WebApplicationBuilder AddSharedServices(this WebApplicationBuilder builder)
    {
        Guard.Against.Null(builder);

        var services = builder.Services;

        services.AddMemoryCache();

        services.AddSingleton<IApplicationInsightResource>(_ => ApplicationInsightResource.Instance);
        services.AddSingleton<IEnvironmentService>(_ => EnvironmentService.Instance);

        services.AddBuildInfo();
        services.AddSnakeSerialization();

        return builder;
    }

    public static WebApplicationBuilder AddEnvironmentChecks(this WebApplicationBuilder builder,
        Assembly[] assemblies, Container container)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrEmpty(assemblies);
        Guard.Against.Null(container);

        builder.Host.ApplyOaktonExtensions();

        container.Collection.Register(typeof(IEnvironmentCheck), assemblies, Lifestyle.Transient);
        container.Collection.Register(typeof(IEnvironmentCheckFactory), assemblies, Lifestyle.Transient);
        container.Collection.Register(typeof(IStatefulResource), assemblies, Lifestyle.Transient);
        container.Collection.Register(typeof(IStatefulResourceSource), assemblies, Lifestyle.Transient);

        return builder;
    }

    public static async Task RunAppAsync(this WebApplication app, string[] args, Container container)
    {
        Guard.Against.Null(app);
        Guard.Against.Null(container);

        var environmentCheckResults = await EnvironmentChecker.ExecuteAllEnvironmentChecks(container)
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
