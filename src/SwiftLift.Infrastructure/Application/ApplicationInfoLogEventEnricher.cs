using Serilog.Core;
using Serilog.Events;
using SwiftLift.SharedKernel.Application;

namespace SwiftLift.Infrastructure.Application;

internal sealed class ApplicationInfoLogEventEnricher(ApplicationInfo applicationInfo)
    : ILogEventEnricher
{
    private LogEventProperty[]? _cachedApplicationLogEventProperties;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        _cachedApplicationLogEventProperties ??= CreateApplicationLogEventProperties(propertyFactory);

        foreach (var property in _cachedApplicationLogEventProperties)
        {
            logEvent.AddPropertyIfAbsent(property);
        }
    }

    private LogEventProperty[] CreateApplicationLogEventProperties(
        ILogEventPropertyFactory propertyFactory)
    {
        return
        [
            propertyFactory.CreateProperty("ApplicationId", applicationInfo.Id),
            propertyFactory.CreateProperty("ApplicationName", applicationInfo.Name),
            propertyFactory.CreateProperty("ApplicationNamespace", applicationInfo.Namespace)
        ];
    }
}
