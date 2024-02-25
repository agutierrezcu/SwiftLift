using FluentValidation.Results;
using NSubstitute.ExceptionExtensions;
using SwiftLift.Infrastructure.BuildInfo;
using SwiftLift.Infrastructure.Serialization;
using Xunit.Extensions.Ordering;

namespace SwiftLift.Infrastructure.UnitTests.BuildInfo;

public class BuildProviderTests
{
    public class WhenMoreThanOneExecutions
    {
        private static readonly CancellationToken s_cancellation = CancellationToken.None;

        private static BuildProvider? s_sut;

        private readonly IBuildInfoLogger _logger;
        private readonly IBuildFileProvider _buildFileProvider;
        private readonly ISnakeJsonDeserializer _jsonSnakeDeserializer;
        private readonly IValidator<Build> _validator;

        private readonly Build _build;
        private readonly string _buildContent;

        public WhenMoreThanOneExecutions()
        {
            // Arrange
            _build = new Build
            {
                Id = "1",
                Number = "1.0.0",
                Branch = "master",
                Commit = "abc123",
                Url = "http://localhost"
            };

            _buildContent = SnakeJsonSerialization.Instance.Serialize(_build);

            _buildFileProvider = Substitute.For<IBuildFileProvider>();
            _buildFileProvider.GetContentAsync(s_cancellation).Returns(_buildContent);

            _jsonSnakeDeserializer = Substitute.For<ISnakeJsonDeserializer>();
            _jsonSnakeDeserializer.Deserialize<Build>(_buildContent).Returns(_build);

            _validator = Substitute.For<IValidator<Build>>();
            _validator.ValidateAsync(_build, s_cancellation).Returns(new ValidationResult());

            _logger = Substitute.For<IBuildInfoLogger>();

            s_sut ??= new BuildProvider(_buildFileProvider, _jsonSnakeDeserializer, _validator, _logger);
        }

        [Fact]
        [Order(1)]
        public async Task Given_ValidBuildContent_When_GetBuildAsync_Then_ReturnValidBuildObject()
        {
            // Act
            var result = await s_sut!.GetBuildAsync(s_cancellation).ConfigureAwait(true);

            // Assert
            result.Should().Be(_build);

            Received.InOrder(() =>
            {
                _buildFileProvider.GetContentAsync(s_cancellation);
                _jsonSnakeDeserializer.Deserialize<Build>(_buildContent);
                _validator.ValidateAsync(_build, s_cancellation);
            });

            await _buildFileProvider
                .Received(1)
                .GetContentAsync(s_cancellation)
                    .ConfigureAwait(true);

            _jsonSnakeDeserializer
                .Received(1)
                .Deserialize<Build>(_buildContent);

            await _validator
                .Received(1)
                .ValidateAsync(_build, s_cancellation)
                    .ConfigureAwait(true);

            _logger
             .DidNotReceiveWithAnyArgs()
             .LogInvalidBuildInfo(Arg.Any<string>(), Arg.Any<string>());

            _logger
                .DidNotReceiveWithAnyArgs()
                .LogUnexpectedExceptionLoadingBuildInfo(Arg.Any<Exception>());
        }

        [Fact]
        [Order(2)]
        public async Task Given_ValidBuildContent_When_GetBuildAsync_Then_ReturnValidBuildObjectFromCache()
        {
            // Arrange
            _buildFileProvider.ClearReceivedCalls();
            _jsonSnakeDeserializer.ClearReceivedCalls();
            _validator.ClearReceivedCalls();
            _logger.ClearReceivedCalls();

            // Act
            var result = await s_sut!.GetBuildAsync(s_cancellation).ConfigureAwait(true);

            // Assert
            result.Should().Be(_build);

            await _buildFileProvider
                .DidNotReceiveWithAnyArgs()
                .GetContentAsync(s_cancellation)
                    .ConfigureAwait(true);

            _jsonSnakeDeserializer
                .DidNotReceiveWithAnyArgs()
                .Deserialize<Build>(_buildContent);

            await _validator
                .DidNotReceiveWithAnyArgs()
                .ValidateAsync(_build, s_cancellation)
                    .ConfigureAwait(true);
            _logger
              .DidNotReceiveWithAnyArgs()
              .LogInvalidBuildInfo(Arg.Any<string>(), Arg.Any<string>());

            _logger
               .DidNotReceiveWithAnyArgs()
               .LogUnexpectedExceptionLoadingBuildInfo(Arg.Any<Exception>());
        }
    }

    [Fact]
    public async Task Given_ValidBuildContent_When_GetBuildAsJsonAsync_Then_ReturnValidBuildJsonString()
    {
        // Arrange
        var cancellation = CancellationToken.None;

        var build = new Build
        {
            Id = "1",
            Number = "1.0.0",
            Branch = "master",
            Commit = "abc123",
            Url = "http://localhost"
        };

        var buildContent = SnakeJsonSerialization.Instance.Serialize(build);

        var buildFileProvider = Substitute.For<IBuildFileProvider>();
        buildFileProvider.GetContentAsync(cancellation).Returns(buildContent);

        var jsonSnakeDeserializer = Substitute.For<ISnakeJsonDeserializer>();
        jsonSnakeDeserializer.Deserialize<Build>(buildContent).Returns(build);

        var validator = Substitute.For<IValidator<Build>>();
        validator.ValidateAsync(build, cancellation).Returns(new ValidationResult());

        var logger = Substitute.For<IBuildInfoLogger>();

        var sut = new BuildProvider(buildFileProvider, jsonSnakeDeserializer, validator, logger);

        // Act
        var result = await sut.GetBuildAsJsonAsync(cancellation).ConfigureAwait(true);

        // Assert
        result.Should().Be(buildContent);

        Received.InOrder(() =>
        {
            buildFileProvider.GetContentAsync(cancellation);
            jsonSnakeDeserializer.Deserialize<Build>(buildContent);
            validator.ValidateAsync(build, cancellation);
        });

        await buildFileProvider
            .Received(1)
            .GetContentAsync(cancellation)
                .ConfigureAwait(true);

        jsonSnakeDeserializer
            .Received(1)
            .Deserialize<Build>(buildContent);

        await validator
            .Received(1)
            .ValidateAsync(build, cancellation)
                .ConfigureAwait(true);

        logger
          .DidNotReceiveWithAnyArgs()
          .LogInvalidBuildInfo(Arg.Any<string>(), Arg.Any<string>());

        logger
            .DidNotReceiveWithAnyArgs()
            .LogUnexpectedExceptionLoadingBuildInfo(Arg.Any<Exception>());
    }

    [Fact]
    public async Task Given_EmptyBuildContent_When_GetBuildAsync_Then_HandleAndLogError()
    {
        // Arrange
        var cancellation = CancellationToken.None;

        var buildFileProvider = Substitute.For<IBuildFileProvider>();

        var exception = new InvalidOperationException("File does not exist");

        buildFileProvider.GetContentAsync(cancellation)
            .ThrowsAsync(exception);

        var jsonSnakeDeserializer = Substitute.For<ISnakeJsonDeserializer>();

        var validator = Substitute.For<IValidator<Build>>();

        var logger = Substitute.For<IBuildInfoLogger>();

        var sut = new BuildProvider(buildFileProvider, jsonSnakeDeserializer, validator, logger);

        // Act
        var result = await sut.GetBuildAsync(cancellation)
                        .ConfigureAwait(true);

        // Assert
        result.Should().BeNull();

        await buildFileProvider
            .Received(1)
            .GetContentAsync(cancellation)
                .ConfigureAwait(true);

        jsonSnakeDeserializer
            .DidNotReceive()
            .Deserialize<Build>(Arg.Any<string>());

        await validator
            .DidNotReceive()
            .ValidateAsync(Arg.Any<Build>(), cancellation)
                .ConfigureAwait(true);

        logger
          .DidNotReceiveWithAnyArgs()
          .LogInvalidBuildInfo(Arg.Any<string>(), Arg.Any<string>());

        logger
            .Received(1)
            .LogUnexpectedExceptionLoadingBuildInfo(exception);
    }

    [Fact]
    public async Task Given_EmptyBuildContent_When_GetBuildAsJsonAsync_Then_HandleAndLogError()
    {
        // Arrange
        var cancellation = CancellationToken.None;

        var buildFileProvider = Substitute.For<IBuildFileProvider>();

        var exception = new InvalidOperationException("File does not exist");

        buildFileProvider.GetContentAsync(cancellation)
            .ThrowsAsync(exception);

        var jsonSnakeDeserializer = Substitute.For<ISnakeJsonDeserializer>();

        var validator = Substitute.For<IValidator<Build>>();

        var logger = Substitute.For<IBuildInfoLogger>();

        var sut = new BuildProvider(buildFileProvider, jsonSnakeDeserializer, validator, logger);

        // Act
        var result = await sut.GetBuildAsJsonAsync(cancellation)
                        .ConfigureAwait(true);

        // Assert
        result
            .Should()
            .Contain("Type: System.InvalidOperationException")
            .And
            .Contain("Message: File does not exist");

        await buildFileProvider
            .Received(1)
            .GetContentAsync(cancellation)
                .ConfigureAwait(true);

        jsonSnakeDeserializer
            .DidNotReceive()
            .Deserialize<Build>(Arg.Any<string>());

        await validator
            .DidNotReceive()
            .ValidateAsync(Arg.Any<Build>(), cancellation)
                .ConfigureAwait(true);

        logger
          .DidNotReceiveWithAnyArgs()
          .LogInvalidBuildInfo(Arg.Any<string>(), Arg.Any<string>());

        logger
            .Received(1)
            .LogUnexpectedExceptionLoadingBuildInfo(exception);
    }

    [Fact]
    public async Task Given_InvalidBuildContent_When_GetBuildAsync_Then_JsonContentIncludesValidationErrors()
    {
        // Arrange
        var cancellation = CancellationToken.None;

        var build = new Build
        {
            Id = "1",
            Number = "1.0.0",
            Branch = "master",
            Commit = "abc123",
            Url = "invalid-url"
        };

        var buildContent = SnakeJsonSerialization.Instance.Serialize(build);

        var buildFileProvider = Substitute.For<IBuildFileProvider>();
        buildFileProvider.GetContentAsync(cancellation).Returns(buildContent);

        var jsonSnakeDeserializer = Substitute.For<ISnakeJsonDeserializer>();
        jsonSnakeDeserializer.Deserialize<Build>(buildContent).Returns(build);

        var validator = new InlineValidator<Build>();
        validator.RuleFor(x => x.Url)
            .Custom((x, context) => context.AddFailure("Invalid Url"));

        var logger = Substitute.For<IBuildInfoLogger>();

        var sut = new BuildProvider(buildFileProvider, jsonSnakeDeserializer, validator, logger);

        // Act
        var result = await sut.GetBuildAsync(cancellation)
                        .ConfigureAwait(true);

        // Assert
        result
            .Should()
            .BeEquivalentTo(build);

        await buildFileProvider
            .Received(1)
            .GetContentAsync(cancellation)
                .ConfigureAwait(true);

        jsonSnakeDeserializer
            .Received(1)
            .Deserialize<Build>(buildContent);

        logger
            .Received(1)
            .LogInvalidBuildInfo(Arg.Any<string>(), Arg.Any<string>());

        logger
           .DidNotReceiveWithAnyArgs()
           .LogUnexpectedExceptionLoadingBuildInfo(Arg.Any<Exception>());
    }

    [Fact]
    public async Task Given_InvalidBuildContent_When_GetBuildAsJsonAsync_Then_JsonContentIncludesValidationErrors()
    {
        // Arrange
        var cancellation = CancellationToken.None;

        var build = new Build
        {
            Id = "1",
            Number = "1.0.0",
            Branch = "master",
            Commit = "abc123",
            Url = "invalid-url"
        };

        var buildContent = SnakeJsonSerialization.Instance.Serialize(build);

        var buildFileProvider = Substitute.For<IBuildFileProvider>();
        buildFileProvider.GetContentAsync(cancellation).Returns(buildContent);

        var jsonSnakeDeserializer = Substitute.For<ISnakeJsonDeserializer>();
        jsonSnakeDeserializer.Deserialize<Build>(buildContent).Returns(build);

        var validator = new InlineValidator<Build>();
        validator.RuleFor(x => x.Url)
            .Custom((x, context) => context.AddFailure("Invalid Url"));

        var logger = Substitute.For<IBuildInfoLogger>();

        var sut = new BuildProvider(buildFileProvider, jsonSnakeDeserializer, validator, logger);

        // Act
        var result = await sut.GetBuildAsJsonAsync(cancellation)
                        .ConfigureAwait(true);

        // Assert
        result
            .Should()
            .Contain("Invalid Build Info")
            .And
            .Contain("Invalid Url");

        await buildFileProvider
            .Received(1)
            .GetContentAsync(cancellation)
                .ConfigureAwait(true);

        jsonSnakeDeserializer
            .Received(1)
            .Deserialize<Build>(buildContent);

        logger
            .Received(1)
            .LogInvalidBuildInfo(Arg.Any<string>(), Arg.Any<string>());

        logger
           .DidNotReceiveWithAnyArgs()
           .LogUnexpectedExceptionLoadingBuildInfo(Arg.Any<Exception>());
    }
}
