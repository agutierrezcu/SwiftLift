using Ardalis.SmartEnum;

namespace SwiftLift.Infrastructure.Logging;

public sealed class OperationEndpoint : SmartEnum<OperationEndpoint, int>
{
    public static readonly OperationEndpoint HealthChecks = new("Health checks", 1);

    public static readonly OperationEndpoint BuildInfo = new("Build info", 2);

    private OperationEndpoint(string name, int value)
        : base(name, value)
    {
    }
}
