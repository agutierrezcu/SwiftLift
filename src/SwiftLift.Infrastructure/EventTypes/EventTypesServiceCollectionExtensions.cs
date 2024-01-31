using Serilog.Core;

namespace SwiftLift.Infrastructure.EventTypes;

public static class EventTypesServiceCollectionExtensions
{
    public static IServiceCollection AddEventTypes(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddSingleton<ILogEventEnricher, EventTypeEnricher>();
        services.AddSingleton<ILogEventEnricher, EventIdNormalizeEnricher>();

        return services;
    }
}
