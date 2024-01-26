namespace SwiftLift.SharedKernel.Serialization;

public interface IJsonSnakeDeserializer
{
    T? Deserialize<T>(string content);

    object? Deserialize(Type objType, string content);
}
