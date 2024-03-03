using System.Diagnostics;

namespace SwiftLift.Infrastructure.Activity;

internal sealed class ActivitySourceDefaultProvider<TActivityStarter> : IActivitySourceProvider<TActivityStarter>
        where TActivityStarter : class
{
    public ActivitySourceDefaultProvider(IHostEnvironment hostEnvironment)
    {
        Guard.Against.Null(hostEnvironment);

        SourceName = $"{hostEnvironment.ApplicationName}.{typeof(TActivityStarter).Name})";

        ActivitySource = new ActivitySource(SourceName);
    }

    public string SourceName { get; }

    public ActivitySource ActivitySource { get; }
}
