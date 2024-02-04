using FastEndpoints;

namespace SwiftLift.Riders.Api;

internal sealed class DummyEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("dummy");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken c)
    {
        return Task.CompletedTask;
    }
}
