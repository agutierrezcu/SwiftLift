using Mono.Cecil;

namespace SwiftLift.Architecture.Tests.Rules;

public sealed class ClassInGeneratedFileExclusionRule : ICustomRule
{
    public static ICustomRule Instance => new ClassInGeneratedFileExclusionRule();

    private ClassInGeneratedFileExclusionRule()
    {
    }

    public bool MeetsRule(TypeDefinition type)
    {
        var typeFilePath = type.GetFilePath();

        var isTypeInGeneratedFile = (typeFilePath?.EndsWith(".g.cs") ?? false) ||
            (typeFilePath?.EndsWith(".generated.cs") ?? false);

        return !isTypeInGeneratedFile;
    }
}
