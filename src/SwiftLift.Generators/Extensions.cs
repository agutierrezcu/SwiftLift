using System.Text;
using System.Text.RegularExpressions;

namespace SwiftLift.Generators;

internal static partial class Extensions
{
    internal static StringBuilder Write(this StringBuilder sb, string? val)
    {
        sb.Append(val);

        return sb;
    }

    static readonly Regex s_regex = new("[^a-zA-Z0-9]+", RegexOptions.Compiled);

    internal static string Sanitize(this string input, string replacement = "_")
    {
        return s_regex.Replace(input, replacement);
    }
}
