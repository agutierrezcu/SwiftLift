namespace SwiftLift.Infrastructure.Correlation;

internal interface ICorrelationIdResolver
{
    bool TryGet([NotNullWhen(true)] out CorrelationId? correlationId);
}
