using System.Collections.Immutable;
using System.Text;

namespace SwiftLift.Generators.ActivitySource;

internal static class SourceGenerationHelper
{
    public const string ActivityStarterAttributeCode =
"""
namespace SwiftLift.Generators.ActivitySource;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ActivityStarterAttribute : Attribute
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
using System.Diagnostics;

namespace {{activitySourceToGenerate.Namespace}};

internal sealed partial class {{activitySourceToGenerate.TypeName}}
{
    private ActivitySource ActivitySource
        => new("{{activitySourceToGenerate.SourceName}}");
}
""");

        return sb.ToString();
    }

    public static string GenerateActivityStarterSourceNameRegister(
        ImmutableArray<ActivityStarterToGenerate?> activityStartersToGenerate)
    {
        var sb = new StringBuilder();

        sb.AppendLine(activityStartersToGenerate.ToString());

//        sb.Append(
//$$"""
//using System.Diagnostics;

//namespace {{activitySourceToGenerate.Namespace}};

//internal sealed partial class {{activitySourceToGenerate.TypeName}}
//{
//    private ActivitySource ActivitySource
//        => new("{{activitySourceToGenerate.SourceName}}");
//}
//""");

        return sb.ToString();
    }
}
