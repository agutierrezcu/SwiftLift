using System.Diagnostics;

namespace SwiftLift.Infrastructure.Activity;

public interface IActivitySourceProvider
{
    string SourceName { get; }

    ActivitySource ActivitySource { get; }
}

public interface IActivitySourceProvider<TActivityStarter> : IActivitySourceProvider
    where TActivityStarter : class
{
}
