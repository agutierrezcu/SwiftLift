using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace SwiftLift.Infrastructure.Serialization;

public static class SerializationServiceCollectionExtensions
{
    public static IServiceCollection AddSnakeSerialization(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.Configure<JsonOptions>(opts => opts.SerializerOptions.Configure());
        services.Configure<MvcJsonOptions>(opts => opts.JsonSerializerOptions.Configure());

        services.TryAddSingleton<ISnakeJsonSerializer, SnakeJsonSerialization>();
        services.TryAddSingleton<ISnakeJsonDeserializer, SnakeJsonSerialization>();

        return services;
    }
}
