using Microsoft.AspNetCore.Builder;
using Oakton;

namespace SwiftLift.Infrastructure.Checks;

public static class EnvironmentCheckWebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddEnvironmentChecks(this WebApplicationBuilder builder,
        Assembly[] applicationAssemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrEmpty(applicationAssemblies);

        builder.Host.ApplyOaktonExtensions();

        var services = builder.Services;

        services
            .Scan(scan => scan
                .FromAssemblies(applicationAssemblies)
                .AddClasses(s => s.AssignableTo<IEnvironmentCheck>(), false)
                .As<IEnvironmentCheck>()
                .WithTransientLifetime());

        services.AddHostedService<EnvironmentChecksHostedService>();

        return builder;
    }
}
