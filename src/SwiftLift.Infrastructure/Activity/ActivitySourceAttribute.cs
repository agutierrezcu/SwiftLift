namespace SwiftLift.Infrastructure.FastEndpoints;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ActivitySourceAttribute : Attribute
{
    public string? ApplicationName { get; set; }

    public string? SourceName { get; set; }
}
