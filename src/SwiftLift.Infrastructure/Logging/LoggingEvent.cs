using Microsoft.Extensions.Logging;

namespace SwiftLift.Infrastructure.Logging;

public static class LoggingEvent
{
    private static int s_sequence = 1000;

    public static EventId ApplicationStarted { get; } = new(s_sequence++, nameof(ApplicationStarted));

    public static EventId ApplicationStopping { get; } = new(s_sequence++, nameof(ApplicationStopping));

    public static EventId ApplicationStopped { get; } = new(s_sequence++, nameof(ApplicationStopped));
}
