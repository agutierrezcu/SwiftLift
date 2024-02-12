using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildEventEnricher(IServiceProvider _serviceProvider)
    : ILogEventEnricher
{
    private static Lazy<List<LogEventProperty>>? s_cachedBuildProperties;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        Guard.Against.Null(logEvent);
        Guard.Against.Null(propertyFactory);

        s_cachedBuildProperties ??= new Lazy<List<LogEventProperty>>(
            () => CreateEventProperties(propertyFactory).ToList());

        foreach (var property in s_cachedBuildProperties.Value)
        {
            logEvent.AddPropertyIfAbsent(property);
        }
    }

    private IEnumerable<LogEventProperty> CreateEventProperties(ILogEventPropertyFactory propertyFactory)
    {
        var buildManager = _serviceProvider.GetRequiredService<IBuildManager>();

        var buildTask = buildManager.GetBuildAsync(default);

        var build = buildTask.GetAwaiter().GetResult();

        yield return propertyFactory.CreateProperty("BuildId", build.Id);

        yield return propertyFactory.CreateProperty("BuildNumber", build.Number);

        yield return propertyFactory.CreateProperty("BuildCommit", build.Commit);
    }
}
