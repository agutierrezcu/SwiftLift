namespace SwiftLift.Infrastructure.Correlation;

internal interface ICorrelationIdResolver
{
    public CorrelationId Resolve();
}
