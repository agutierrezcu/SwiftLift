namespace SwiftLift.Infrastructure.Environment;

public interface IEnvironmentService
{
    string? GetVariable(string name);
}
