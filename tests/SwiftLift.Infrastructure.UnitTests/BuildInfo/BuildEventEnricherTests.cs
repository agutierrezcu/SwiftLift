using Serilog.Core;
using Serilog.Events;
using SwiftLift.Infrastructure.BuildInfo;
using Xunit.Extensions.Ordering;

namespace SwiftLift.Infrastructure.UnitTests.BuildInfo;

public sealed class BuildEventEnricherTests
{
    [Fact]
    public void Given_NullLogEvent_When_Enrich_Then_ThrowArgumentNullException()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();

        var sut = new BuildEventEnricher(serviceProvider);

        // Act
        Action act = () => sut.Enrich(null, null);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'logEvent')");
    }

    [Fact]
    public void Given_NullPropertyFactory_When_Enrich_Then_ThrowArgumentNullException()
    {
        // Arrange
        var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate(string.Empty, []), []);

        var serviceProvider = Substitute.For<IServiceProvider>();

        var sut = new BuildEventEnricher(serviceProvider);

        //Act
        Action act = () => sut.Enrich(logEvent, null);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'propertyFactory')");
    }

    [Fact, Order(1)]
    public void Given_LogEvent_When_Enrich_Then_BuildPropertiesAreAdded()
    {
        // Arrange
        var build = new Build
        {
            Id = "1",
            Number = "1.0.0",
            Branch = "master",
            Commit = "abc123",
            Url = "http://localhost"
        };

        var propertyFactory = Substitute.For<ILogEventPropertyFactory>();

        propertyFactory.CreateProperty(Arg.Any<string>(), Arg.Any<object?>())
           .Returns(callInfo => new LogEventProperty((string)callInfo[0], new ScalarValue(callInfo[1])));

        var buildManager = Substitute.For<IBuildManager>();

        buildManager.GetBuildAsync(default).Returns(Task.FromResult(build));

        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(typeof(IBuildManager)).Returns(buildManager);

        var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null,
            new MessageTemplate(string.Empty, []), []);

        var sut = new BuildEventEnricher(serviceProvider);

        // Act
        sut.Enrich(logEvent, propertyFactory);

        // Assert
        Received.InOrder(() =>
        {
            serviceProvider.GetService(typeof(IBuildManager));

            buildManager.GetBuildAsync(default);

            propertyFactory.CreateProperty("BuildId", build.Id);
            propertyFactory.CreateProperty("BuildNumber", build.Number);
            propertyFactory.CreateProperty("BuildCommit", build.Commit);
        });

        var properties = logEvent.Properties;

        var expectedProperties = new List<LogEventProperty>
        {
            new("BuildId", new ScalarValue(build.Id)),
            new("BuildNumber", new ScalarValue(build.Number)),
            new("BuildCommit", new ScalarValue(build.Commit))
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
        var build = new Build
        {
            Id = "1",
            Number = "1.0.0",
            Branch = "master",
            Commit = "abc123",
            Url = "http://localhost"
        };

        var propertyFactory = Substitute.For<ILogEventPropertyFactory>();

        var buildManager = Substitute.For<IBuildManager>();

        var serviceProvider = Substitute.For<IServiceProvider>();

        var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null,
            new MessageTemplate(string.Empty, []), []);

        var sut = new BuildEventEnricher(serviceProvider);

        // Act
        sut.Enrich(logEvent, propertyFactory);

        // Assert
        serviceProvider
            .DidNotReceiveWithAnyArgs()
            .GetService(typeof(IBuildManager));

        buildManager
            .DidNotReceiveWithAnyArgs()
            .GetBuildAsync(default);

        propertyFactory
            .DidNotReceiveWithAnyArgs()
            .CreateProperty(Arg.Any<string>(), Arg.Any<object?>());

        var properties = logEvent.Properties;

        var expectedProperties = new List<LogEventProperty>
        {
            new("BuildId", new ScalarValue(build.Id)),
            new("BuildNumber", new ScalarValue(build.Number)),
            new("BuildCommit", new ScalarValue(build.Commit))
        };

        foreach (var expectedProperty in expectedProperties)
        {
            properties.Should().ContainKey(expectedProperty.Name);
            properties[expectedProperty.Name].Should().Be(expectedProperty.Value);
        }
    }
}
