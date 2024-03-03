using OpenTelemetry.Trace;
using Scrutor;
using SwiftLift.Infrastructure.Activity;

namespace SwiftLift.Infrastructure.FastEndpoints;

internal class EndpointActivitySourceSubscriber(IHostEnvironment _environment)
    : RegistrationStrategy
{
    public override void Apply(IServiceCollection services, ServiceDescriptor descriptor)
    {
        Guard.Against.Null(services);
        Guard.Against.Null(descriptor);

        var endpointType = descriptor.ServiceType;

        var activitySourceProviderType =
            typeof(ActivitySourceDefaultProvider<>).MakeGenericType(endpointType);

        var activitySourceProvider =
            Activator.CreateInstance(activitySourceProviderType, _environment)
                as IActivitySourceProvider;

        services.ConfigureOpenTelemetryTracerProvider(
            tracing => tracing.AddSource(activitySourceProvider!.SourceName));
    }
}
