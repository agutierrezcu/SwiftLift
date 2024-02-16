using Serilog.Core;
using Serilog.Events;

namespace SwiftLift.Infrastructure.BuildInfo;

internal sealed class BuildEventEnricher(IServiceProvider _serviceProvider)
    : ILogEventEnricher
{
    private static Lazy<Task<List<LogEventProperty>>>? s_cachedBuildProperties;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        Guard.Against.Null(logEvent);
        Guard.Against.Null(propertyFactory);

        s_cachedBuildProperties ??= new Lazy<Task<List<LogEventProperty>>>(
            () => CreateLogEventPropertiesAsync(propertyFactory));

        foreach (var property in s_cachedBuildProperties.Value.Result)
        {
            logEvent.AddPropertyIfAbsent(property);
        }
    }

    private async Task<List<LogEventProperty>> CreateLogEventPropertiesAsync(
        ILogEventPropertyFactory propertyFactory)
    {
        var buildProvider = _serviceProvider.GetRequiredService<IBuildProvider>();

        var build = await buildProvider.GetBuildAsync(default)
            .ConfigureAwait(false);

        return
        [
            propertyFactory.CreateProperty("BuildId", build.Id),
            propertyFactory.CreateProperty("BuildNumber", build.Number),
            propertyFactory.CreateProperty("BuildCommit", build.Commit)
        ];
    }
}
