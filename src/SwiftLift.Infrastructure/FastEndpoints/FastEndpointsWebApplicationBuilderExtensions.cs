using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using SwiftLift.Infrastructure.Activity;

namespace SwiftLift.Infrastructure.FastEndpoints;

public static class FastEndpointsWebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddFastEndpoints(this WebApplicationBuilder builder,
        params Assembly[] applicationAssemblies)
    {
        Guard.Against.Null(builder);
        Guard.Against.NullOrEmpty(applicationAssemblies);

        var services = builder.Services;

        services
            .AddFastEndpoints(
                opts =>
                {
                    //opts.
                })
            .SwaggerDocument();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddActivitySourceProvider();

        return builder;
    }
}
