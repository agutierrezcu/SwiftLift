using Microsoft.AspNetCore.Hosting;
using Serilog.Core;

namespace SwiftLift.Infrastructure.Correlation;

public static class CorrelationIdServiceCollectionExtensions
{
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddSingleton<ILogEventEnricher, CorrelationIdManager>();
        services.AddSingleton<ICorrelationIdResolver, CorrelationIdManager>();

        services.AddHeaderPropagation(options =>
        {
            options.Headers.Add(
                CorrelationIdHeader.Name,
                context =>
                {
                    return string.IsNullOrWhiteSpace(context.HeaderValue)
                        ? Guid.NewGuid().ToString()
                        : context.HeaderValue;
                });
        });

        services.AddTransient<IStartupFilter, CorrelationIdStartupFilter>();
        services.AddScoped<CorrelationIdResponseMiddleware>();

        return services;
    }
}
