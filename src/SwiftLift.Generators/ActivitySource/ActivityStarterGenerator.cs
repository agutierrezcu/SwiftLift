using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using static SwiftLift.Generators.ActivitySource.SourceGenerationHelper;

namespace SwiftLift.Generators.ActivitySource;

[Generator(LanguageNames.CSharp)]
public class ActivityStarterGenerator : IIncrementalGenerator
{
    private const string AttributeName = "ActivityStarterAttribute";

    private const string FullyQualifiedAttributeName = "SwiftLift.Generators.ActivitySource.ActivityStarterAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(
            ctx => ctx.AddSource(
                "ActivityStarterAttribute.g.cs",
                SourceText.From(
                    ActivityStarterAttributeCode, Encoding.UTF8)));

        var activityStartersToGenerate =
            context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    FullyQualifiedAttributeName,
                    predicate: static (s, _) => true,
                    transform: static (ctx, ct) =>
                        GetActivityStarterToGenerate(ctx, ct))
                .Where(static m => m is not null)
                .Collect();

        context.RegisterSourceOutput(activityStartersToGenerate,
            static (spc, source) => Execute(spc, source));
    }

    private static ActivityStarterToGenerate? GetActivityStarterToGenerate(
        GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
        {
            // nothing to do if this type isn't available
            return null;
        }

        ct.ThrowIfCancellationRequested();

        var nameSpace =
            typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : typeSymbol.ContainingNamespace.ToString();

        var name = typeSymbol.Name;

        var sourceName = $"{typeSymbol.ContainingAssembly.Name}.{typeSymbol.Name}";

        foreach (var attributeData in typeSymbol.GetAttributes())
        {
            var attributeClass = attributeData.AttributeClass;

            if (attributeClass?.Name != AttributeName ||
                    attributeClass?.ToDisplayString() != FullyQualifiedAttributeName)
            {
                continue;
            }

            foreach (var namedArgument in attributeData.NamedArguments)
            {
                if (namedArgument.Key != "SourceName")
                {
                    continue;
                }

                if (namedArgument.Value.Value?.ToString() is not { } sn)
                {
                    break;
                }

                sourceName = sn;
                break;
            }
        }

        return new(nameSpace, name, sourceName);
    }

    private static void Execute(SourceProductionContext context,
        ImmutableArray<ActivityStarterToGenerate?> activityStartersToGenerate)
    {
        foreach (var activityStarterToGenerate in activityStartersToGenerate)
        {
            if (activityStarterToGenerate is not { } activityStarter)
            {
                return;
            }

            var partialActivityStarter = GeneratePartialClassWithActivitySource(activityStarter);

            context.AddSource($"{activityStarter.Namespace}.{activityStarter.TypeName}.g.cs",
                SourceText.From(partialActivityStarter, Encoding.UTF8));
        }

        GenerateActivityStarterSourceNameRegister(activityStartersToGenerate);
        //context.AddSource($"{value.Namespace}.{value.TypeName}.g.cs",
        //    SourceText.From(activityStarterRegistrations, Encoding.UTF8));
    }
}
