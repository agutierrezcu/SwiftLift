namespace SwiftLift.SharedKernel.Serialization;

public interface IJsonSnakeSerializer
{
    string Serialize<T>(T instance);
}
