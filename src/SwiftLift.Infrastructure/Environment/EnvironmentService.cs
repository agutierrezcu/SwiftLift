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

    public string GetRequiredVariable(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);

        var environmentVariable = GetVariable(name);

        if (string.IsNullOrWhiteSpace(environmentVariable))
        {
            throw new InvalidOperationException("Environment variable is not defined or value is not set.");
        }

        return environmentVariable;
    }
}
