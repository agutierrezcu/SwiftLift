using static System.StringComparison;
using static SwiftLift.Infrastructure.ConnectionString.ConnectionStringDefaults;

namespace SwiftLift.Infrastructure.ConnectionString;

internal static class ConnectionStringParser
{
    public static ConnectionStringResource Parse(
        string? resourceName,
        string? connectionString,
        string? segmentSeparator = SegmentSeparator,
        string? keywordValueSeparator = KeywordValueSeparator,
        bool allowEmptyValues = false)
    {
        Guard.Against.NullOrWhiteSpace(resourceName);
        Guard.Against.NullOrWhiteSpace(connectionString);
        Guard.Against.NullOrWhiteSpace(segmentSeparator);
        Guard.Against.NullOrWhiteSpace(keywordValueSeparator);

        IDictionary<string, string> segments = ParseSegments(resourceName, connectionString,
            segmentSeparator, keywordValueSeparator, allowEmptyValues);

        return new ConnectionStringResource(resourceName, connectionString, segments);
    }

    private static Dictionary<string, string> ParseSegments(
        in string resourceName,
        in string connectionString,
        in string segmentSeparator,
        in string keywordValueSeparator,
        in bool allowEmptyValues = false)
    {
        Dictionary<string, string>? segments = new(StringComparer.OrdinalIgnoreCase);

        var segmentStart = -1;
        var segmentEnd = 0;

        while (TryGetNextSegment(connectionString, segmentSeparator, ref segmentStart, ref segmentEnd))
        {
            ThrowInvalidExceptionIfInvalid(resourceName, connectionString,
                segmentSeparator, keywordValueSeparator,
                allowEmptyValues, segmentStart, segmentEnd);

            var kvSeparatorIndex = connectionString.IndexOf
                (keywordValueSeparator, segmentStart, segmentEnd - segmentStart, Ordinal);
            var keywordStart = GetStart(connectionString, segmentStart);
            var keyLength = GetLength(connectionString, keywordStart, kvSeparatorIndex);

            var keyword = connectionString.Substring(keywordStart, keyLength);

            if (segments.ContainsKey(keyword))
            {
                throw new InvalidConnectionStringException(
                    resourceName, $"Duplicated keyword '{keyword}'");
            }

            var valueStart = GetStart(connectionString, kvSeparatorIndex + keywordValueSeparator.Length);
            var valueLength = GetLength(connectionString, valueStart, segmentEnd);
            segments.Add(keyword, connectionString.Substring(valueStart, valueLength));
        }

        return segments;

        static int GetStart(in string str, int start)
        {
            while (start < str.Length && char.IsWhiteSpace(str[start]))
            {
                start++;
            }

            return start;
        }

        static int GetLength(in string str, in int start, int end)
        {
            while (end > start && char.IsWhiteSpace(str[end - 1]))
            {
                end--;
            }

            return end - start;
        }
    }

    private static bool TryGetNextSegment(
        in string str,
        in string segmentSeparator,
        ref int start,
        ref int end)
    {
        if (start == -1)
        {
            start = 0;
        }
        else
        {
            start = end + segmentSeparator.Length;

            if (start >= str.Length)
            {
                return false;
            }
        }

        end = str.IndexOf(segmentSeparator, start, Ordinal);

        if (end == -1)
        {
            end = str.Length;
        }

        return true;
    }

    private static void ThrowInvalidExceptionIfInvalid(
        string resourceName,
        string connectionString,
        string segmentSeparator,
        string keywordValueSeparator,
        bool allowEmptyValues,
        int segmentStart,
        int segmentEnd)
    {
        if (segmentStart == segmentEnd)
        {
            if (segmentStart == 0)
            {
                throw new InvalidConnectionStringException(
                    resourceName, $"Connection string starts with segment separator '{segmentSeparator}'");
            }

            throw new InvalidConnectionStringException(
                resourceName, $"Connection string contains two following segment separators '{segmentSeparator}'");
        }

        var kvSeparatorIndex = connectionString.IndexOf(keywordValueSeparator,
            segmentStart, segmentEnd - segmentStart, Ordinal);

        if (kvSeparatorIndex == -1)
        {
            throw new InvalidConnectionStringException(
                resourceName, $"Connection string doesn't have value for keyword '{connectionString[segmentStart..segmentEnd]}'");
        }

        if (segmentStart == kvSeparatorIndex)
        {
            throw new InvalidConnectionStringException(
                resourceName, $"Connection string has value '{connectionString[(segmentStart + 1)..segmentEnd]}' with no keyword");
        }

        if (!allowEmptyValues && kvSeparatorIndex + 1 == segmentEnd)
        {
            throw new InvalidConnectionStringException(
                resourceName, $"Connection string has keyword '{connectionString[segmentStart..kvSeparatorIndex]}' with empty value");
        }
    }
}
