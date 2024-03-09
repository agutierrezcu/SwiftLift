using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using static SwiftLift.Generators.ActivitySource.SourceGenerationHelper;

namespace SwiftLift.Generators.ActivitySource;

[Generator(LanguageNames.CSharp)]
public class ActivityStarterGenerator : IIncrementalGenerator
{
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
                    ActivityStarterAttributeFullyQualifiedName,
                    predicate: static (s, _) => true,
                    transform: static (ctx, ct) =>
                        GetActivityStarterToGenerate(ctx, ct))
                .Where(static m => m is not null)
                .Collect();

        context.RegisterSourceOutput(activityStartersToGenerate,
            static (spc, source) => ExecuteActivityStarterCode(spc, source));
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

        var sourceName = typeSymbol.ToString();

        foreach (var attributeData in typeSymbol.GetAttributes())
        {
            var attributeClass = attributeData.AttributeClass;

            if (attributeClass?.Name != ActivityStarterAttributeName ||
                    attributeClass?.ToDisplayString() != ActivityStarterAttributeFullyQualifiedName)
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

    private static void ExecuteActivityStarterCode(SourceProductionContext context,
        ImmutableArray<ActivityStarterToGenerate?> activityStartersToGenerate)
    {
        var sourceNames = new HashSet<string>();

        foreach (var activityStarterToGenerate in activityStartersToGenerate)
        {
            if (activityStarterToGenerate is not { } activityStarter)
            {
                continue;
            }

            var partialActivityStarterCode = GeneratePartialClassWithActivitySource(activityStarter);

            sourceNames.Add(activityStarter.SourceName);

            context.AddSource($"{activityStarter.Namespace}.{activityStarter.TypeName}.g.cs",
                SourceText.From(partialActivityStarterCode, Encoding.UTF8));
        }

        var templateText = GetEmbededResource(
            "SwiftLift.Generators.ActivitySource.Templates.AddSourceNamesTemplate.scriban");

        var template = Template.Parse(templateText);

        var sourceCode = template.Render(new
        {
            Sources = sourceNames.ToArray()
        });

        context.AddSource(
            "OpenTelemetryTracerSourceNamesServiceCollectionExtensions.g.cs",
            SourceText.From(sourceCode, Encoding.UTF8)
        );
    }

    private static string GetEmbededResource(string path)
    {
        using var stream = typeof(ActivityStarterGenerator).Assembly
            .GetManifestResourceStream(path);

        using var streamReader = new StreamReader(stream);

        return streamReader.ReadToEnd();
    }
}
