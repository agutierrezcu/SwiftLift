using ExpectedObjects;
using Microsoft.Extensions.Logging;

namespace SwiftLift.Infrastructure.UnitTests.Extensions;

public static class ILoggerExtensions
{
    public static void ReceivedMatchingArgs(this ILogger logger, int requiredNumberOfCalls,
        LogLevel logLevel, string formattedMessage)
    {
        logger
            .Received(requiredNumberOfCalls)
            .Log(Arg.Is(logLevel),
                Arg.Is<EventId>(0),
                Arg.Is<object>(x => x.ToString() == formattedMessage),
                Arg.Is<Exception>(x => x == null),
                Arg.Any<Func<object, Exception?, string>>());
    }

    public static void ReceivedMatchingArgs(this ILogger logger, int requiredNumberOfCalls,
        LogLevel logLevel, Exception exception, string formattedMessage)
    {
        if (exception == null)
        {
            logger.ReceivedMatchingArgs(requiredNumberOfCalls, logLevel, formattedMessage);
        }

        var expectedException = exception.ToExpectedObject();

        logger
            .Received(requiredNumberOfCalls)
            .Log(Arg.Is(logLevel),
                Arg.Is<EventId>(0),
                Arg.Is<object>(x => x.ToString() == formattedMessage),
                Arg.Is<Exception>(x => expectedException.Equals(x)),
                Arg.Any<Func<object, Exception?, string>>());
    }

}
