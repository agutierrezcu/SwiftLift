using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Oakton.Environment;
using SwiftLift.SharedKernel.ApplicationInsight;
using SwiftLift.SharedKernel.Environment;

namespace SwiftLift.ServiceDefaults;

public static partial class Extensions
{
    public static WebApplicationBuilder AddSharedServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services
            .AddSingleton<IApplicationInsightResource>(_ => ApplicationInsightResource.Instance)
            .AddSingleton<IEnvironmentService>(_ => EnvironmentService.Instance);

        return builder;
    }

    public static WebApplicationBuilder AddEnvironmentChecks(this WebApplicationBuilder builder)
    {
        builder.Host.ApplyOaktonExtensions();

        var services = builder.Services;

        // TODO: Refactoring by SimpleInjector discovery feature
        services.AddSingleton<IEnvironmentCheck, ApplicationInsightEnvironmentCheck>();

        return builder;
    }

    public static async Task RunAppAsync(this WebApplication app, string[] args)
    {
        var environmentCheckResults = await EnvironmentChecker.ExecuteAllEnvironmentChecks(app.Services)
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
