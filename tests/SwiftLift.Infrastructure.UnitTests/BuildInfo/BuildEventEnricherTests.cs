using Serilog.Core;
using Serilog.Events;
using SwiftLift.Infrastructure.BuildInfo;
using Xunit.Extensions.Ordering;

namespace SwiftLift.Infrastructure.UnitTests.BuildInfo;

public class BuildEventEnricherFixture
{
    public IServiceProvider ServiceProvider { get; private set; }

    public Build Build { get; private set; }

    public ILogEventPropertyFactory PropertyFactory { get; private set; }

    public IBuildProvider BuildProvider { get; private set; }

    internal BuildLogEventEnricher Sut { get; private set; }

    public BuildEventEnricherFixture()
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

public sealed class BuildEventEnricherTests(BuildEventEnricherFixture _sutFixture)
    : IClassFixture<BuildEventEnricherFixture>
{
    [Fact, Order(1)]
    public void Given_LogEvent_When_Enrich_Then_BuildPropertiesAreAdded()
    {
        // Arrange
        var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null,
            new MessageTemplate(string.Empty, []), []);

        // Act
        _sutFixture.Sut.Enrich(logEvent, _sutFixture.PropertyFactory);

        // Assert
        Received.InOrder(() =>
        {
            _sutFixture.ServiceProvider.GetService(typeof(IBuildProvider));

            _sutFixture.BuildProvider.GetBuildAsync(default);

            _sutFixture.PropertyFactory.CreateProperty("BuildId", _sutFixture.Build.Id);
            _sutFixture.PropertyFactory.CreateProperty("BuildNumber", _sutFixture.Build.Number);
            _sutFixture.PropertyFactory.CreateProperty("BuildCommit", _sutFixture.Build.Commit);
        });

        _sutFixture.ServiceProvider
            .Received(1)
            .GetService(typeof(IBuildProvider));

        _sutFixture.BuildProvider
            .Received(1)
            .GetBuildAsync(default);

        _sutFixture.PropertyFactory
            .Received(1)
            .CreateProperty("BuildId", _sutFixture.Build.Id);

        _sutFixture.PropertyFactory
            .Received(1)
            .CreateProperty("BuildNumber", _sutFixture.Build.Number);

        _sutFixture.PropertyFactory
            .Received(1)
            .CreateProperty("BuildCommit", _sutFixture.Build.Commit);

        var properties = logEvent.Properties;

        var expectedProperties = new List<LogEventProperty>
        {
            new("BuildId", new ScalarValue(_sutFixture.Build.Id)),
            new("BuildNumber", new ScalarValue(_sutFixture.Build.Number)),
            new("BuildCommit", new ScalarValue(_sutFixture.Build.Commit))
        };

        foreach (var expectedProperty in expectedProperties)
        {
            properties.Should().ContainKey(expectedProperty.Name);
            properties[expectedProperty.Name].Should().Be(expectedProperty.Value);
        }
    }

    [Fact, Order(2)]
    public void Given_LogEvent_When_Enrich_Then_BuildPropertiesAreAddedFromCache()
    {
        // Arrange
        _sutFixture.BuildProvider.ClearReceivedCalls();
        _sutFixture.PropertyFactory.ClearReceivedCalls();
        _sutFixture.ServiceProvider.ClearReceivedCalls();
        _sutFixture.BuildProvider.ClearReceivedCalls();

        var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null,
            new MessageTemplate(string.Empty, []), []);

        // Act
        _sutFixture.Sut.Enrich(logEvent, _sutFixture.PropertyFactory);

        // Assert
        _sutFixture.ServiceProvider
            .DidNotReceiveWithAnyArgs()
            .GetService(typeof(IBuildProvider));

        _sutFixture.BuildProvider
            .DidNotReceiveWithAnyArgs()
            .GetBuildAsync(default);

        _sutFixture.PropertyFactory
            .DidNotReceiveWithAnyArgs()
            .CreateProperty(Arg.Any<string>(), Arg.Any<object?>());

        var properties = logEvent.Properties;

        var expectedProperties = new List<LogEventProperty>
        {
            new("BuildId", new ScalarValue(_sutFixture.Build.Id)),
            new("BuildNumber", new ScalarValue(_sutFixture.Build.Number)),
            new("BuildCommit", new ScalarValue(_sutFixture.Build.Commit))
        };

        foreach (var expectedProperty in expectedProperties)
        {
            properties.Should().ContainKey(expectedProperty.Name);
            properties[expectedProperty.Name].Should().Be(expectedProperty.Value);
        }
    }
}
