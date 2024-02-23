using Mono.Cecil;

namespace SwiftLift.Architecture.Tests.Rules;

public static class TypeDefinitionExtensions
{
    public static PredicateList AreNotInGeneratedFile(this Predicate predicate)
    {
        return predicate.MeetCustomRule(ClassInGeneratedFileExclusionRule.Instance);
    }

    public static string? GetFilePath(this TypeDefinition typeDefinition)
    {
        var method = typeDefinition.Methods.FirstOrDefault(m => m.DebugInformation.HasSequencePoints);
        return method?.DebugInformation.SequencePoints.FirstOrDefault()?.Document.Url;
    }
}
