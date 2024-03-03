using System.Diagnostics;

namespace SwiftLift.Infrastructure.Activity;

internal sealed class ActivitySourceProvider<TActivityStarter> : IActivitySourceProvider<TActivityStarter>
        where TActivityStarter : class
{
    public ActivitySourceProvider(IHostEnvironment hostEnvironment)
    {
        Guard.Against.Null(hostEnvironment);

        SourceName = $"{hostEnvironment.ApplicationName}.{typeof(TActivityStarter).Name})";

        ActivitySource = new ActivitySource(SourceName);
    }

    public string SourceName { get; }

    public ActivitySource ActivitySource { get; }
}
