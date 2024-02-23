using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdLogEventEnricher(ICorrelationIdResolver _correlationIdResolver)
    : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!_correlationIdResolver.TryGet(out var correlationId))
        {
            return;
        }

        var correlationIdProperty = propertyFactory.CreateProperty(
            "CorrelationId", correlationId);

        logEvent.AddPropertyIfAbsent(correlationIdProperty!);
    }
}
