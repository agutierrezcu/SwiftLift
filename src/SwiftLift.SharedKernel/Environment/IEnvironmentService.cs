namespace SwiftLift.SharedKernel.Environment;

public interface IEnvironmentService
{
    string? GetVariable(string name);
}
