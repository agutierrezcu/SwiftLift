using static System.Globalization.CultureInfo;

namespace SwiftLift.Infrastructure.Exceptions;

public static class ExceptionExtension
{
    public static string GetDetails(this Exception exception, StringBuilder? sb = null)
    {
        if (exception == null)
        {
            return sb?.ToString() ?? string.Empty;
        }

        if (sb == null)
        {
            sb = new StringBuilder();
            sb.AppendLine();
        }

        sb.AppendLine(InvariantCulture, $"Type: {exception.GetType()}");
        sb.AppendLine(InvariantCulture, $"Message: {exception.Message}");
        sb.AppendLine(InvariantCulture, $"Source: {exception.Source}");

        if (exception.Data?.Count > 0)
        {
            sb.AppendLine("Data:");
            foreach (DictionaryEntry entry in exception.Data)
            {
                sb.AppendLine(InvariantCulture, $"{entry.Key}={entry.Value}");
            }
        }

        sb.AppendLine(InvariantCulture, $"StackTrace: {exception.StackTrace}");

        sb.AppendLine((exception.InnerException != null)
            ? "********* Inner exception: *********"
            : "********* No inner exception: *********");

        exception.InnerException?.GetDetails(sb);
        return sb.ToString();
    }
}
