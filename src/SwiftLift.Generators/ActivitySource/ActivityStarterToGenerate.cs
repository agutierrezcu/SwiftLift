namespace SwiftLift.Generators.ActivitySource;

public readonly record struct ActivityStarterToGenerate(
    string Namespace,
    string TypeName,
    string SourceName)
{
    public readonly string Namespace = Namespace;

    public readonly string TypeName = TypeName;

    public readonly string SourceName = SourceName;
}
