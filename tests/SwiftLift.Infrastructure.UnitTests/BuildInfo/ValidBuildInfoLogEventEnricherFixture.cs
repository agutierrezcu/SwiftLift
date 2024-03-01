using Serilog.Core;
using Serilog.Events;
using SwiftLift.Infrastructure.BuildInfo;

namespace SwiftLift.Infrastructure.UnitTests.BuildInfo;

public class ValidBuildInfoLogEventEnricherFixture
{
    public IServiceProvider ServiceProvider { get; private set; }

    public Build Build { get; private set; }

    public ILogEventPropertyFactory PropertyFactory { get; private set; }

    public IBuildProvider BuildProvider { get; private set; }

    internal BuildLogEventEnricher Sut { get; private set; }

    public ValidBuildInfoLogEventEnricherFixture()
    {
        // Arrange
        Build = new Build
        {
            Id = "1",
            Number = "1.0.0",
            Branch = "master",
            Commit = "abc123",
            Url = "http://localhost"
        };

        PropertyFactory = Substitute.For<ILogEventPropertyFactory>();

        PropertyFactory.CreateProperty(Arg.Any<string>(), Arg.Any<object?>())
           .Returns(callInfo => new LogEventProperty((string)callInfo[0], new ScalarValue(callInfo[1])));

        BuildProvider = Substitute.For<IBuildProvider>();

        BuildProvider.GetBuildAsync(default).Returns(Task.FromResult(Build));

        ServiceProvider = Substitute.For<IServiceProvider>();

        ServiceProvider.GetService(typeof(IBuildProvider)).Returns(BuildProvider);

        Sut = new BuildLogEventEnricher(ServiceProvider);
    }
}
