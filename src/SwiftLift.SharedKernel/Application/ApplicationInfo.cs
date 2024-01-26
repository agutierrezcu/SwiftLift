using Ardalis.GuardClauses;

namespace SwiftLift.SharedKernel.Application;

[ExcludeFromCodeCoverage]
public sealed record ApplicationInfo
{
    public ApplicationInfo(string id, string name, string @namespace)
    {
        Guard.Against.NullOrWhiteSpace(id);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NullOrWhiteSpace(@namespace);

        Id = id;
        Name = name;
        Namespace = @namespace;
    }

    public string Id { get; }

    public string Name { get; }

    public string Namespace { get; }
}
