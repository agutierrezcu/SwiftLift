using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SwiftLift.Generators.ActivitySource;

[Generator(LanguageNames.CSharp)]
public class ActivitySourceGenerator : IIncrementalGenerator
{
    private static readonly StringBuilder s_b = new();

    private const string AttribShortName = "ActivitySource";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
                          .CreateSyntaxProvider(Qualify, Transform)
                          .Where(static m => m.IsInvalid is false)
                          .WithComparer(new Comparer())
                          .Collect();

        context.RegisterSourceOutput(provider, Generate);

        static bool Qualify(SyntaxNode node, CancellationToken _)
        {
            return node is ClassDeclarationSyntax { TypeParameterList: null } cds &&
                       cds.AttributeLists.Any(al => al.Attributes.Any(a => a.Name is GenericNameSyntax { Identifier.ValueText: AttribShortName }));
        }

        static Match Transform(GeneratorSyntaxContext ctx, CancellationToken _)
        {
            //should be re-assigned on every call. do not cache!
            var assemblyName = ctx.SemanticModel.Compilation.AssemblyName;

            return new(ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, cancellationToken: _), (ClassDeclarationSyntax)ctx.Node);
        }
    }

    private static void Generate(SourceProductionContext ctx, ImmutableArray<Match> matches)
    {
        if (matches.Length == 0)
        {
            return;
        }

        var activitySources = matches.Select(static m => new ActivitySourceRegistration(m));

        //s_b.Clear().Write(
        //    $$"""
        //      namespace {{s_assemblyName}};

        //      using Microsoft.Extensions.DependencyInjection;

        //      public static class ServiceRegistrationExtensions
        //      {
        //          public static IServiceCollection RegisterServicesFrom{{s_assemblyName?.Sanitize(string.Empty) ?? "Assembly"}}(this IServiceCollection sc)
        //          {

        //      """);

        //foreach (var reg in activitySources.OrderBy(r => r!.LifeTime).ThenBy(r => r!.ServiceType))
        //{
        //    s_b.Write(
        //        $"""
        //                 sc.Add{reg!.LifeTime}<{reg.ServiceType}, {reg.ImplType}>();

        //         """);
        //}
        //s_b.Write(
        //    """
            
        //            return sc;
        //        }
        //    }
        //    """);

        ctx.AddSource("ServiceRegistrations.g.cs", SourceText.From(s_b.ToString(), Encoding.UTF8));
    }

    private readonly struct Match(ISymbol? symbol, ClassDeclarationSyntax classDec)
    {
        public ISymbol? Symbol { get; } = symbol;
        public ClassDeclarationSyntax ClassDec { get; } = classDec;
        public bool IsInvalid => Symbol?.IsAbstract is null or true;
    }

    private sealed class Comparer : IEqualityComparer<Match>
    {
        public bool Equals(Match x, Match y)
        {
            return x.Symbol!.ToDisplayString().Equals(y.Symbol!.ToDisplayString()) &&
                       x.ClassDec.AttributeLists.ToString().Equals(y.ClassDec.AttributeLists.ToString());
        }

        public int GetHashCode(Match obj)
        {
            return obj.Symbol!.ToDisplayString().GetHashCode();
        }
    }

    private readonly struct ActivitySourceRegistration(Match match)
    {
        public string? SourceName { get; } = match.Symbol?.ToDisplayString();
    }
}
