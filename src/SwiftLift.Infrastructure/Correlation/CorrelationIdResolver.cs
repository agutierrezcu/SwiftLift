using Microsoft.AspNetCore.HeaderPropagation;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdResolver(HeaderPropagationValues _headerPropagationValues)
    : ICorrelationIdResolver
{
    public bool TryGet(out CorrelationId? correlationId)
    {
        if (_headerPropagationValues.Headers is null ||
                !_headerPropagationValues.Headers.TryGetValue(
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
