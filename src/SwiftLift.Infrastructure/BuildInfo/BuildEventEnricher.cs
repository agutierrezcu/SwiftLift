using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildEventEnricher(IServiceProvider serviceProvider)
    : ILogEventEnricher
{
    private static List<LogEventProperty>? s_buildProperties;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        s_buildProperties ??= CreateEventProperties(propertyFactory).ToList();

        foreach (var property in s_buildProperties)
        {
            logEvent.AddPropertyIfAbsent(property);
        }
    }

    private IEnumerable<LogEventProperty> CreateEventProperties(ILogEventPropertyFactory propertyFactory)
    {
        var buildManager = serviceProvider.GetRequiredService<IBuildManager>();

        var buildTask = buildManager.GetBuildAsync(default);

        var build = buildTask.GetAwaiter().GetResult();

        yield return propertyFactory.CreateProperty("BuildId", build.Id);
        yield return propertyFactory.CreateProperty("BuildNumber", build.Number);
        yield return propertyFactory.CreateProperty("BuildCommit", build.Commit);
    }
}
