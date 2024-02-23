namespace SwiftLift.Infrastructure.Correlation;

[StronglyTypedId(
    backingType: StronglyTypedIdBackingType.Guid,
    jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
internal partial struct CorrelationId
{
}
