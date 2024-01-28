namespace SwiftLift.Infrastructure.Serialization;

[ExcludeFromCodeCoverage]
internal sealed class JsonTextSnakeSerialization : IJsonSnakeSerializer, IJsonSnakeDeserializer
{
    public static readonly JsonTextSnakeSerialization Instance = new();

    private static readonly JsonSerializerOptions s_serializerOptions = new();

    static JsonTextSnakeSerialization()
    {
        s_serializerOptions.Configure();
    }

    public string Serialize<T>(T instance)
    {
        return JsonSerializer.Serialize(instance, s_serializerOptions);
    }

    public T? Deserialize<T>(string content)
    {
        return JsonSerializer.Deserialize<T>(content, s_serializerOptions);
    }

    public object? Deserialize(Type objType, string content)
    {
        return JsonSerializer.Deserialize(content, objType, s_serializerOptions);
    }
}
