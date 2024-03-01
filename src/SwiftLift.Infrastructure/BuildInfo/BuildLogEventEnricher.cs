using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildLogEventEnricher(IServiceProvider _serviceProvider)
    : ILogEventEnricher
{
    private LogEventProperty[]? _cachedBuildLogEventProperties;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        _cachedBuildLogEventProperties ??= CreateBuildLogEventProperties(propertyFactory);

        foreach (var property in _cachedBuildLogEventProperties)
        {
            logEvent.AddPropertyIfAbsent(property);
        }
    }

    private LogEventProperty[] CreateBuildLogEventProperties(
        ILogEventPropertyFactory propertyFactory)
    {
        var buildProvider = _serviceProvider.GetRequiredService<IBuildProvider>();

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        var build = buildProvider.GetBuildAsync(default).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

        if (build is null)
        {
            return [];
        }

        return
        [
            propertyFactory.CreateProperty("BuildId", build.Id),
            propertyFactory.CreateProperty("BuildNumber", build.Number),
            propertyFactory.CreateProperty("BuildCommit", build.Commit)
        ];
    }
}
