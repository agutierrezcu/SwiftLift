using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SwiftLift.Infrastructure.Serialization;

public static class SerializationServiceCollectionExtensions
{
    public static IServiceCollection AddSnakeSerialization(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.TryAddSingleton<ISnakeJsonSerializer, SnakeJsonSerialization>();
        services.TryAddSingleton<ISnakeJsonDeserializer, SnakeJsonSerialization>();

        return services;
    }
}
