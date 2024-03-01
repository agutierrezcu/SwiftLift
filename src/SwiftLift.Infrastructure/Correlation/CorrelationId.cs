namespace SwiftLift.Infrastructure.Correlation;

[StronglyTypedId(
    backingType: StronglyTypedIdBackingType.Guid,
    jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
internal partial struct CorrelationId
{
    public const string HeaderName = "Correlation-Id";

    public const string LogEventPropertyName = "CorrelationId";
}
