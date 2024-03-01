using Microsoft.AspNetCore.Hosting;

namespace SwiftLift.Infrastructure.Correlation;

public static class CorrelationIdServiceCollectionExtensions
{
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.TryAddSingleton<ICorrelationIdResolver, CorrelationIdResolver>();

        services.AddHeaderPropagation(options =>
        {
            options.Headers.Add(
                CorrelationId.HeaderName,
                context =>
                {
                    var correlationIdResolver = context.HttpContext
                        .RequestServices.GetRequiredService<ICorrelationIdResolver>();

                    return string.IsNullOrWhiteSpace(context.HeaderValue)
                        ? correlationIdResolver.Resolve().ToString()
                        : context.HeaderValue;
                });
        });

        services.AddScoped<CorrelationIdResponseMiddleware>();

        services.AddTransient<IStartupFilter, CorrelationIdStartupFilter>();

        return services;
    }
}
