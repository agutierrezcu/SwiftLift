using System.Text;

namespace SwiftLift.Generators.ActivitySource;

internal static class SourceGenerationHelper
{
    internal const string ActivityStarterAttributeNameSpace = "SwiftLift.Generators.ActivitySource";

    internal const string ActivityStarterAttributeName = "ActivityStarterAttribute";

    internal const string ActivityStarterAttributeFullyQualifiedName =
        $"{ActivityStarterAttributeNameSpace}.{ActivityStarterAttributeName}";

    public const string ActivityStarterAttributeCode =
$$"""
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace {{ActivityStarterAttributeNameSpace}};

[ExcludeFromCodeCoverage]
[CompilerGenerated]
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class {{ActivityStarterAttributeName}} : Attribute
{
    public string SourceName { get; set; }
}
""";

    public static string GeneratePartialClassWithActivitySource(
        ActivityStarterToGenerate activitySourceToGenerate)
    {
        var sb = new StringBuilder();

        sb.Append(
$$"""
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace {{activitySourceToGenerate.Namespace}};

[ExcludeFromCodeCoverage]
[CompilerGenerated]
partial class {{activitySourceToGenerate.TypeName}}
{
    private ActivitySource ActivitySource
        => new("{{activitySourceToGenerate.SourceName}}");
}
""");

        return sb.ToString();
    }
}
