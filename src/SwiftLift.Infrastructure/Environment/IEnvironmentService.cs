namespace SwiftLift.Infrastructure.Environment;

public interface IEnvironmentService
{
    string? GetVariable(string name);

    string GetRequiredVariable(string name);
}
