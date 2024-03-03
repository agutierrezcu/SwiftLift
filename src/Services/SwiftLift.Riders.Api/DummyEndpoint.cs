using FastEndpoints;
using SwiftLift.Infrastructure.Activity;
using SwiftLift.Infrastructure.FastEndpoints;

namespace SwiftLift.Riders.Api;

[ActivitySource]
internal sealed partial class DummyEndpoint(IActivitySourceProvider<DummyEndpoint> _activitySourceProvider)
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
