namespace SwiftLift.SharedKernel.Serialization;

public static class SnakeStringExtensionMethods
{
    public static string ToSnake(this string value)
    {
        Guard.Against.NullOrWhiteSpace(value);

        return string.Concat(
            value.Select(
                (x, i) => i > 0 && char.IsUpper(x)
                    ? JsonSnakeCaseNamingPolicy.Separator.ToString() + x
                    : x.ToString()
                    )
            ).ToLower();
    }

    public static string ToUpperSnake(this string value)
    {
        Guard.Against.NullOrWhiteSpace(value);

        return value.ToSnake().ToUpper();
    }
}
