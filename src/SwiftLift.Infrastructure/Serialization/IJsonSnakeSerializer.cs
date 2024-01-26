namespace SwiftLift.Infrastructure.Serialization;

public interface IJsonSnakeSerializer
{
    string Serialize<T>(T instance);
}
