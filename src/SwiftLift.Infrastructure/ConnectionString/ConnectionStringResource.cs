namespace SwiftLift.Infrastructure.ConnectionString;

public sealed record ConnectionStringResource
{
    public ConnectionStringResource(string name, string value, IDictionary<string, string> segments)
    {
        Name = Guard.Against.NullOrWhiteSpace(name);
        Value = Guard.Against.NullOrWhiteSpace(value);

        Guard.Against.NullOrEmpty(segments);

        Segments = segments;
    }

    public string Name { get; }

    public string Value { get; }

    public IDictionary<string, string> Segments { get; }

    public bool TryGetSegmentValue(string keyword, out string? value)
    {
        Guard.Against.NullOrWhiteSpace(keyword);

        return Segments.TryGetValue(keyword, out value);
    }
}
