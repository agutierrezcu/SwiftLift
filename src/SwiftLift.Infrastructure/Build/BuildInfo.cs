namespace SwiftLift.Infrastructure.Build;

[ExcludeFromCodeCoverage]
public sealed record BuildInfo
{
    public required string Id { get; init; }

    public required string Number { get; init; }

    public required string Branch { get; init; }

    public required string Commit { get; init; }

    public required string Url { get; init; }
}

