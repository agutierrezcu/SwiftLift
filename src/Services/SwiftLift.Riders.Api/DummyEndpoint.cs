using FastEndpoints;

namespace SwiftLift.Riders.Api;

internal sealed class DummyEndpoint(ILogger<DummyEndpoint> _logger)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("dummy");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken c)
    {
        _logger.LogInformation("Executing Dummy endpoint");

        return Task.CompletedTask;
    }
}
