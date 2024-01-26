namespace SwiftLift.SharedKernel.Serialization;

internal sealed class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
{
    public static readonly JsonNamingPolicy Instance = new JsonSnakeCaseNamingPolicy();

    public const char Separator = '_';

    private JsonSnakeCaseNamingPolicy()
    {
    }

    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        ReadOnlySpan<char> spanName = name.Trim();

        var stringBuilder = new StringBuilder();
        var addCharacter = true;
        var isNextLower = false;
        var isNextUpper = false;
        var isNextSpace = false;

        for (var position = 0; position < spanName.Length; position++)
        {
            if (position != 0)
            {
                var isCurrentSpace = spanName[position] == 32;
                var isPreviousSpace = spanName[position - 1] == 32;
                var isPreviousSeparator = spanName[position - 1] == 95;

                if (position + 1 != spanName.Length)
                {
                    isNextLower = spanName[position + 1] is > (char)96 and < (char)123;
                    isNextUpper = spanName[position + 1] is > (char)64 and < (char)91;
                    isNextSpace = spanName[position + 1] == 32;
                }

                if (isCurrentSpace &&
                    (isPreviousSpace ||
                    isPreviousSeparator ||
                    isNextUpper ||
                    isNextSpace))
                {
                    addCharacter = false;
                }
                else
                {
                    var isCurrentUpper = spanName[position] is > (char)64 and < (char)91;
                    var isPreviousLower = spanName[position - 1] is > (char)96 and < (char)123;
                    var isPreviousNumber = spanName[position - 1] is > (char)47 and < (char)58;

                    if (isCurrentUpper &&
                    (isPreviousLower ||
                    isPreviousNumber ||
                    isNextLower ||
                    isNextSpace ||
                    (isNextLower && !isPreviousSpace)))
                    {
                        stringBuilder.Append(Separator);
                    }
                    else
                    {
                        if (isCurrentSpace &&
                            !isPreviousSpace &&
                            !isNextSpace)
                        {
                            stringBuilder.Append(Separator);
                            addCharacter = false;
                        }
                    }
                }
            }

            if (addCharacter)
            {
                stringBuilder.Append(spanName[position]);
            }
            else
            {
                addCharacter = true;
            }
        }

        return stringBuilder.ToString().ToLower();
    }
}
