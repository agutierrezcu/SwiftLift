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

            http.AddHeaderPropagation(
                opts => opts.Headers.Add(CorrelationId.HeaderName));
        });

        return services;
    }
}
