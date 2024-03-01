using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdLogEventEnricher(ICorrelationIdResolver _correlationIdResolver)
    : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = _correlationIdResolver.Resolve();

        var correlationIdProperty = propertyFactory.CreateProperty(
            "CorrelationId", correlationId);

        logEvent.AddPropertyIfAbsent(correlationIdProperty!);
    }
}
