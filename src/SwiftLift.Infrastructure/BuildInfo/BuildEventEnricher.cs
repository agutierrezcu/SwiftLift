using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildEventEnricher(IServiceProvider _serviceProvider)
    : ILogEventEnricher
{
    private Lazy<List<LogEventProperty>>? _cachedBuildProperties;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        _cachedBuildProperties ??= new Lazy<List<LogEventProperty>>(
            () => CreateEventProperties(propertyFactory).ToList());

        foreach (var property in _cachedBuildProperties.Value)
        {
            logEvent.AddPropertyIfAbsent(property);
        }
    }

    private IEnumerable<LogEventProperty> CreateEventProperties(ILogEventPropertyFactory propertyFactory)
    {
        var buildManager = _serviceProvider.GetRequiredService<IBuildManager>();

        var buildTask = buildManager.GetBuildAsync(default);

        var build = buildTask.GetAwaiter().GetResult();

        yield return propertyFactory.CreateProperty(
            "BuildId", new ScalarValue(build.Id));

        yield return propertyFactory.CreateProperty(
            "BuildNumber", new ScalarValue(build.Number));

        yield return propertyFactory.CreateProperty(
            "BuildCommit", new ScalarValue(build.Commit));
    }
}
