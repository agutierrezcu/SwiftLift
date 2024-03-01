using System.Collections.Concurrent;
using Murmur;
using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.EventTypes;

internal sealed class EventTypeLogEventEnricher : ILogEventEnricher
{
    private readonly ConcurrentDictionary<string, Lazy<LogEventProperty>>
        _cachedMessageTemplateEventType = new();

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var eventTypeProperty = _cachedMessageTemplateEventType.GetOrAdd(
            logEvent.MessageTemplate.Text,
            messageTemplate =>
                new Lazy<LogEventProperty>(
                    () =>
                    {
                        var murmur = MurmurHash.Create32();
                        var bytes = Encoding.UTF8.GetBytes(logEvent.MessageTemplate.Text);

                        var hash = murmur.ComputeHash(bytes);
                        var numericHash = BitConverter.ToUInt32(hash, 0);

                        return propertyFactory.CreateProperty("EventType", numericHash);
                    }));

        logEvent.AddPropertyIfAbsent(eventTypeProperty.Value);
    }
}
