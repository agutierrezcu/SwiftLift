namespace SwiftLift.Infrastructure.Environment;

public sealed class EnvironmentService : IEnvironmentService
{
    public static readonly EnvironmentService Instance = new();

    private EnvironmentService()
    {
    }

    public string? GetVariable(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);

        return System.Environment.GetEnvironmentVariable(name);
    }
}
