using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Oakton;

namespace SwiftLift.Infrastructure.Checks;

public static class EnvironmentCheckExtensions
{
    public static WebApplicationBuilder AddEnvironmentChecks(this WebApplicationBuilder builder,
        Assembly[] assemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrEmpty(assemblies);

        builder.Host.ApplyOaktonExtensions();

        var services = builder.Services;

        services.AddTransient<IStartupFilter, EnvironmentCheckStartupFilter>();

        services
            .Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(s => s.AssignableTo<IEnvironmentCheck>(), false)
                .As<IEnvironmentCheck>()
                .WithTransientLifetime()
            );

        return builder;
    }
}
