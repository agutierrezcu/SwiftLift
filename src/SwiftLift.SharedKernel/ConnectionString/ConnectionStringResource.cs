namespace SwiftLift.SharedKernel.ConnectionString;

public sealed record ConnectionStringResource
{
    public ConnectionStringResource(string name, string value, IDictionary<string, string> segments)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(value);
        Guard.Against.Null(segments);

        Name = name;
        Value = value;
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
