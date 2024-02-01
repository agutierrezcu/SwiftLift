namespace SwiftLift.Infrastructure.Correlation;

internal interface ICorrelationIdResolver
{
    bool TryGet(out CorrelationId? correlationId);
}
