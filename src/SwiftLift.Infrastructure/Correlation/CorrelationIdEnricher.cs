using Microsoft.AspNetCore.HeaderPropagation;
using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdEnricher(HeaderPropagationValues headerPropagationValues)
    : ILogEventEnricher
{
    private LogEventProperty? _cachedCorrelationIdProperty;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        _cachedCorrelationIdProperty ??=
            CreateCorrelationIdProperty(headerPropagationValues, propertyFactory);

        if (_cachedCorrelationIdProperty is null)
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(_cachedCorrelationIdProperty);
    }

    private static LogEventProperty? CreateCorrelationIdProperty(
        HeaderPropagationValues headerPropagationValues,
        ILogEventPropertyFactory propertyFactory)
    {
        Guard.Against.Null(headerPropagationValues);
        Guard.Against.Null(propertyFactory);

        if (headerPropagationValues.Headers is null)
        {
            return null;
        }

        if (!headerPropagationValues.Headers
                .TryGetValue(CorrelationIdHeader.Name, out var correlationId))
        {
            return null;
        }

        var correlationIdValue = correlationId.FirstOrDefault() ?? "Not set";

        return propertyFactory.CreateProperty("CorrelationId", correlationIdValue);
    }
}
