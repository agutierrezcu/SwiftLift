using FastEndpoints;
using SwiftLift.Infrastructure.Activity;

namespace SwiftLift.Riders.Api;

internal sealed class DummyEndpoint(IActivitySourceProvider<DummyEndpoint> _activitySourceProvider)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("dummy");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySourceProvider.ActivitySource
            .StartActivity("Handle Dummy Endpoint");

        Logger.LogInformation("Executing Dummy endpoint");

        return Task.CompletedTask;
    }
}
