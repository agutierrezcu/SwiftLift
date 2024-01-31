using MassTransit;
using Serilog.Core;

namespace SwiftLift.Infrastructure.Correlation;

public static class CorrelationIdServiceCollectionExtensions
{
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddSingleton<ILogEventEnricher, CorrelationIdEnricher>();

        services.AddHeaderPropagation(options =>
        {
            options.Headers.Add(
                CorrelationIdHeader.Name,
                context => NewId.NextGuid().ToString());
        });

        return services;
    }
}
