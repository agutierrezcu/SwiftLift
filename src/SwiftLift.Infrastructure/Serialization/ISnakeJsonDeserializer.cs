namespace SwiftLift.Infrastructure.Serialization;

public interface ISnakeJsonDeserializer
{
    T? Deserialize<T>(string content);

    object? Deserialize(Type objType, string content);
}
