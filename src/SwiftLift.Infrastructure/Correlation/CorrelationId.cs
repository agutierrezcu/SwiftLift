namespace SwiftLift.Infrastructure.Correlation;

[StronglyTypedId(
    backingType: StronglyTypedIdBackingType.String,
    jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
internal partial struct CorrelationId
{
}
