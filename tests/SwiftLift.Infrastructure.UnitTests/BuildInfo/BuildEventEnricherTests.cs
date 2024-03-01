using Serilog.Core;
using Serilog.Events;
using SwiftLift.Infrastructure.BuildInfo;
using Xunit.Extensions.Ordering;

namespace SwiftLift.Infrastructure.UnitTests.BuildInfo;

public sealed class BuildEventEnricherTests
{
    [Fact]
    public void Given_NullBuildObject_When_Enrich_Then_NoPropertiesAdded()
    {
        // Arrange
        var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null,
            new MessageTemplate(string.Empty, []), []);

        var buildProvider = Substitute.For<IBuildProvider>();

        Build? build = null;
        buildProvider.GetBuildAsync(default).ReturnsForAnyArgs(Task.FromResult<Build>(build));

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IBuildProvider)).Returns(buildProvider);

        var propertyFactory = Substitute.For<ILogEventPropertyFactory>();

        var sut = new BuildLogEventEnricher(serviceProvider);

        // Act
        sut.Enrich(logEvent, propertyFactory);

        // Assert
        Received.InOrder(() =>
        {
            serviceProvider.GetService(typeof(IBuildProvider));
            buildProvider.GetBuildAsync(default);
        });

        serviceProvider
            .Received(1)
            .GetService(typeof(IBuildProvider));

        buildProvider
            .Received(1)
            .GetBuildAsync(default);

        propertyFactory
            .DidNotReceiveWithAnyArgs()
            .CreateProperty(Arg.Any<string>(), Arg.Any<object>());

        logEvent.Properties.Should().BeEmpty();
    }

    public sealed class BuildEventEnricherFixturedTests(ValidBuildInfoLogEventEnricherFixture _sutFixture)
    : IClassFixture<ValidBuildInfoLogEventEnricherFixture>
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
}
