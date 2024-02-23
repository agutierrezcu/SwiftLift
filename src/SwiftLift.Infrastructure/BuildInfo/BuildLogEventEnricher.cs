using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildLogEventEnricher(IServiceProvider _serviceProvider)
    : ILogEventEnricher
{
    private List<LogEventProperty>? _cachedBuildProperties;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        _cachedBuildProperties ??= CreateBuildLogEventProperties(propertyFactory);

        foreach (var property in _cachedBuildProperties)
        {
            logEvent.AddPropertyIfAbsent(property);
        }
    }

    private List<LogEventProperty> CreateBuildLogEventProperties(
        ILogEventPropertyFactory propertyFactory)
    {
        var buildProvider = _serviceProvider.GetRequiredService<IBuildProvider>();

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        var build = buildProvider.GetBuildAsync(default).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

        return
        [
            propertyFactory.CreateProperty("BuildId", build.Id),
            propertyFactory.CreateProperty("BuildNumber", build.Number),
            propertyFactory.CreateProperty("BuildCommit", build.Commit)
        ];
    }
}
