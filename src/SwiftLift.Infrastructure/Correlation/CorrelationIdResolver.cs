using Microsoft.AspNetCore.HeaderPropagation;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdResolver(HeaderPropagationValues _headerPropagationValues)
    : ICorrelationIdResolver
{
    public bool TryGet([NotNullWhen(true)] out CorrelationId? correlationId)
    {
        if (_headerPropagationValues.Headers?.TryGetValue(
                CorrelationIdHeader.Name, out var correlationIdHeader) ?? false)
        {
            var correlationIdValue = correlationIdHeader.FirstOrDefault() ?? "Not set";

            correlationId = new(correlationIdValue);
            return true;
        }

        correlationId = null;
        return false;
    }
}
