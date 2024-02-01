namespace SwiftLift.Infrastructure.Serialization;

public interface ISnakeJsonSerializer
{
    string Serialize<T>(T instance);
}
