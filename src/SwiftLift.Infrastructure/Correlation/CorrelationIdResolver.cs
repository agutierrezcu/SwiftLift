using Microsoft.AspNetCore.HeaderPropagation;
using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdResolver(
    HeaderPropagationValues _headerPropagationValues)
        : ILogEventEnricher,
          ICorrelationIdResolver
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        Guard.Against.Null(logEvent);
        Guard.Against.Null(propertyFactory);

        if (!TryGet(out var correlationId))
        {
            return;
        }

        var correlationIdProperty = propertyFactory.CreateProperty(
            "CorrelationId", new ScalarValue(correlationId));

        logEvent.AddPropertyIfAbsent(correlationIdProperty!);
    }

    public bool TryGet(out CorrelationId? correlationId)
    {
        if (_headerPropagationValues.Headers is null)
        {
            correlationId = null;
            return false;
        }

        if (!_headerPropagationValues.Headers.TryGetValue(
                CorrelationIdHeader.Name, out var correlationIdHeader))
        {
            correlationId = null;
            return false;
        }

        var correlationIdValue = correlationIdHeader.FirstOrDefault() ?? "Not set";

        correlationId = new(correlationIdValue);
        return true;
    }
}
