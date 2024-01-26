using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SwiftLift.SharedKernel.Serialization;

public static class SerializationServiceCollectionExtensions
{
    public static IServiceCollection AddSnakeSerialization(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.TryAddSingleton<IJsonSnakeSerializer, JsonTextSnakeSerialization>();
        services.TryAddSingleton<IJsonSnakeDeserializer, JsonTextSnakeSerialization>();

        return services;
    }
}
