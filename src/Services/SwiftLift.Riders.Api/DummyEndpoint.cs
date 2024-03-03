using FastEndpoints;
using SwiftLift.Generators.ActivitySource;

namespace SwiftLift.Riders.Api;

[ActivityStarter]
internal sealed partial class DummyEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("dummy");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Handle Dummy Endpoint");

        Logger.LogInformation("Executing Dummy endpoint");

        return Task.CompletedTask;
    }
}
