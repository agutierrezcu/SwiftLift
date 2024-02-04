using Ardalis.SmartEnum;

namespace SwiftLift.Infrastructure.Logging;

public sealed class ExcludedLoggingEndpoint : SmartEnum<ExcludedLoggingEndpoint, int>
{
    public static readonly ExcludedLoggingEndpoint HealthChecks = new("Health checks", 1);

    public static readonly ExcludedLoggingEndpoint BuildInfo = new("Build info", 2);

    private ExcludedLoggingEndpoint(string name, int value)
        : base(name, value)
    {
    }
}
