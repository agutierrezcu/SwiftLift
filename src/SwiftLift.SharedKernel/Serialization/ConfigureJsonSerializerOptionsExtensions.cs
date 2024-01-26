using System.Text.Json.Serialization;

namespace SwiftLift.SharedKernel.Serialization;

public static class ConfigureJsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions Configure(this JsonSerializerOptions serializerOptions)
    {
        Guard.Against.Null(serializerOptions);

        serializerOptions.Converters.Add(new JsonStringEnumConverter());

        serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        serializerOptions.DictionaryKeyPolicy = JsonSnakeCaseNamingPolicy.Instance;
        serializerOptions.PropertyNamingPolicy = JsonSnakeCaseNamingPolicy.Instance;

        return serializerOptions;
    }
}
