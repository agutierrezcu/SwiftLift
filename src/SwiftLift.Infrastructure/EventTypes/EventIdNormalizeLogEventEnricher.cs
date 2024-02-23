using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.EventTypes;

internal sealed class EventIdNormalizeLogEventEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.TryGetValue(nameof(EventId), out var eventIdPropertyValue) ||
                eventIdPropertyValue is not StructureValue structuredEventId)
        {
            return;
        }

        var currentEventIdProperty = structuredEventId.Properties
            .SingleOrDefault(p => p.Name == nameof(EventId.Id));

        if (currentEventIdProperty is null)
        {
            return;
        }

        var eventIdProperty = propertyFactory.CreateProperty(
            "EventId", currentEventIdProperty.Value);

        logEvent.AddOrUpdateProperty(eventIdProperty);

        var currentEventNameProperty = structuredEventId.Properties
            .SingleOrDefault(p => p.Name == nameof(EventId.Name));

        if (currentEventNameProperty is null)
        {
            return;
        }

        var eventNameValue = currentEventNameProperty.Value
            .ToString().Replace("\"", string.Empty);

        var eventNameProperty = propertyFactory.CreateProperty(
            "EventName", eventNameValue);

        logEvent.AddOrUpdateProperty(eventNameProperty);
    }
}
