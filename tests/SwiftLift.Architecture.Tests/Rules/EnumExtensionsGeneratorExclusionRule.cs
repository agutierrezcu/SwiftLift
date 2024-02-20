using Mono.Cecil;

namespace SwiftLift.Architecture.Tests.Rules;

public sealed class EnumExtensionsGeneratorExclusionRule : ICustomRule
{
    public static ICustomRule Instance => new EnumExtensionsGeneratorExclusionRule();

    private EnumExtensionsGeneratorExclusionRule()
    {
    }

    public bool MeetsRule(TypeDefinition type)
    {
        var isEnumExtension = type.GetFilePath()?.EndsWith("_EnumExtensions.g.cs") ?? false;

        return !isEnumExtension;
    }
}
