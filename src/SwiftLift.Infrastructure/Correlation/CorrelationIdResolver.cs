using Microsoft.AspNetCore.Http;

namespace SwiftLift.Infrastructure.Correlation;

internal sealed class CorrelationIdResolver(IHttpContextAccessor _httpContextAccessor)
    : ICorrelationIdResolver
{
    private static readonly AsyncLocal<CorrelationId> s_correlationId = new();

    public CorrelationId Resolve()
    {
        if (s_correlationId.Value != default)
        {
            return s_correlationId.Value;
        }

        if (_httpContextAccessor.HttpContext is null)
        {
            s_correlationId.Value = CorrelationId.New();
            return s_correlationId.Value;
        }

        var request = _httpContextAccessor.HttpContext.Request;

        if (!request.Headers.TryGetValue(CorrelationId.HeaderName,
                out var correlationIdHeader) || correlationIdHeader.Count != 1)
        {
            s_correlationId.Value = CorrelationId.New();
            return s_correlationId.Value;
        }

        if (!Guid.TryParse(correlationIdHeader.ToString(), out var correlationIdGuid))
        {
            s_correlationId.Value = CorrelationId.New();
            return s_correlationId.Value;
        }

        s_correlationId.Value = new(correlationIdGuid);
        return s_correlationId.Value;
    }
}
