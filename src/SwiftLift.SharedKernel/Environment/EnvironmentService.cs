using Ardalis.GuardClauses;

namespace SwiftLift.SharedKernel.Environment;

public sealed class EnvironmentService : IEnvironmentService
{
    public static EnvironmentService Instance = new();

    private EnvironmentService()
    {
    }

    public string? GetVariable(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);

        return System.Environment.GetEnvironmentVariable(name);
    }
}
