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
                CorrelationIdHeader.Name,
                context =>
                {
                    return string.IsNullOrWhiteSpace(context.HeaderValue)
                        ? CorrelationId.New().ToString()
                        : context.HeaderValue;
                });
        });

        services.AddScoped<CorrelationIdResponseMiddleware>();

        services.AddTransient<IStartupFilter, CorrelationIdStartupFilter>();

        return services;
    }
}
