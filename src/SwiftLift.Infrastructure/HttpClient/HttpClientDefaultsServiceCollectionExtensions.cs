using Microsoft.AspNetCore.HeaderPropagation;
using Microsoft.Extensions.Primitives;
using SwiftLift.Infrastructure.Correlation;

namespace SwiftLift.Infrastructure.HttpClient;

public static class HttpClientDefaultsServiceCollectionExtensions
{
    public static IServiceCollection ConfigureHttpClient(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddServiceDiscovery();

        services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.UseServiceDiscovery();

            http.AddHttpContextHeaderPropagation(
                opts => opts.Headers.Add(CorrelationId.HeaderName));
        });

        return services;
    }

    private static IHttpClientBuilder AddHttpContextHeaderPropagation(this IHttpClientBuilder builder,
        Action<HeaderPropagationMessageHandlerOptions>? configure)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(configure);

        builder.Services.AddHeaderPropagation();

        builder.AddHttpMessageHandler(serviceProvider =>
        {
            var headerPropagationMessageHandlerOptions = new HeaderPropagationMessageHandlerOptions();
            configure?.Invoke(headerPropagationMessageHandlerOptions);

            var headerPropagationValues = serviceProvider.GetRequiredService<HeaderPropagationValues>();

            headerPropagationValues.Headers ??= new Dictionary<string, StringValues>();

            return new HeaderPropagationMessageHandler(headerPropagationMessageHandlerOptions, headerPropagationValues);
        });

        return builder;
    }
}
