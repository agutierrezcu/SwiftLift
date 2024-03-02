using System.Diagnostics;
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

    private readonly ActivitySource _source = new("SwiftLift.Api.Dummy");

    public override Task HandleAsync(CancellationToken c)
    {
        using var activity = _source.StartActivity("Executing dummy endpoint");

        activity?.SetTag("tag1", "tag1");
        activity?.SetTag("tag2", "tag2");

        _logger.LogInformation("Executing Dummy endpoint");

        return Task.CompletedTask;
    }
}
