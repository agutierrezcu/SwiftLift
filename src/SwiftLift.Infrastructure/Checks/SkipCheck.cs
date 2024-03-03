namespace SwiftLift.Infrastructure.Checks;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal sealed class SkipCheckAttribute : Attribute
{
}
