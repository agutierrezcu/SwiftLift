using NSubstitute.ExceptionExtensions;
using SwiftLift.Infrastructure.BuildInfo;
using SwiftLift.Infrastructure.Serialization;

namespace SwiftLift.Infrastructure.UnitTests.BuildInfo;

public class BuildEnvironmentCheckTests
{
    private readonly BuildEnvironmentCheck _sut = new();

    [Fact]
    public void Given_Check_When_ReadDescription_Then_NotEmpty()
    {
        // Arrange

        // Act
        var description = _sut.Description;

        // Assert
        description.Should().NotBeEmpty();
    }

    [Fact]
    public void Given_Check_When_ServiceProviderIsNul_Then_ThrowArgumentNullException()
    {
        // Arrange
        IServiceProvider? serviceProvider = null;

        // Act
        Func<Task> act = () => _sut.Assert(serviceProvider, CancellationToken.None);

        // Assert
        act.Should()
            .ThrowExactlyAsync<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'services')");
    }

    [Fact]
    public async Task Given_Check_When_AssertCalled_Then_CallExpectedServices_And_NoExceptionIsThrown()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var serviceProvider = Substitute.For<IServiceProvider>();

        var buildFileProvider = Substitute.For<IBuildFileProvider>();
        serviceProvider.GetService(typeof(IBuildFileProvider)).Returns(buildFileProvider);

        var snakeJsonDeserializer = Substitute.For<ISnakeJsonDeserializer>();
        serviceProvider.GetService(typeof(ISnakeJsonDeserializer)).Returns(snakeJsonDeserializer);

        var buildValidator = Substitute.For<IValidator<Build>>();
        serviceProvider.GetService(typeof(IValidator<Build>)).Returns(buildValidator);

        // Act
        await _sut.Assert(serviceProvider, cancellationToken)
            .ConfigureAwait(true);

        // Assert
        Received.InOrder(() =>
        {
            serviceProvider.GetService(typeof(IBuildFileProvider));
            serviceProvider.GetService(typeof(ISnakeJsonDeserializer));
            serviceProvider.GetService(typeof(IValidator<Build>));

            buildFileProvider.GetContentAsync(cancellationToken);
            snakeJsonDeserializer.Deserialize<Build>(Arg.Any<string>());
            buildValidator.ValidateAsync(Arg.Any<IValidationContext>(), cancellationToken);
        });
    }

    [Fact]
    public async Task Given_Check_When_GetFileContentFail_Then_ThrowException()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var serviceProvider = Substitute.For<IServiceProvider>();

        var buildFileProvider = Substitute.For<IBuildFileProvider>();

        buildFileProvider
            .GetContentAsync(cancellationToken)
            .ThrowsAsync(new InvalidOperationException("Get file content failed"));

        serviceProvider.GetService(typeof(IBuildFileProvider)).Returns(buildFileProvider);

        var snakeJsonDeserializer = Substitute.For<ISnakeJsonDeserializer>();
        serviceProvider.GetService(typeof(ISnakeJsonDeserializer)).Returns(snakeJsonDeserializer);

        var buildValidator = Substitute.For<IValidator<Build>>();
        serviceProvider.GetService(typeof(IValidator<Build>)).Returns(buildValidator);

        // Act
        Func<Task> act = () => _sut.Assert(serviceProvider, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Get file content failed")
            .ConfigureAwait(true);

        Received.InOrder(() =>
        {
            serviceProvider.GetService(typeof(IBuildFileProvider));
            serviceProvider.GetService(typeof(ISnakeJsonDeserializer));
            serviceProvider.GetService(typeof(IValidator<Build>));

            buildFileProvider.GetContentAsync(cancellationToken);
        });

        snakeJsonDeserializer
            .DidNotReceiveWithAnyArgs()
            .Deserialize<Build>(Arg.Any<string>());

        await buildValidator
            .DidNotReceiveWithAnyArgs()
            .ValidateAsync(Arg.Any<IValidationContext>(), cancellationToken)
                .ConfigureAwait(true);
    }

    [Fact]
    public async Task Given_Check_When_DeserializeFail_Then_ThrowException()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var serviceProvider = Substitute.For<IServiceProvider>();

        var buildFileProvider = Substitute.For<IBuildFileProvider>();

        buildFileProvider
            .GetContentAsync(cancellationToken)
            .Returns("build.json");

        serviceProvider.GetService(typeof(IBuildFileProvider)).Returns(buildFileProvider);

        var snakeJsonDeserializer = Substitute.For<ISnakeJsonDeserializer>();

        snakeJsonDeserializer
            .Deserialize<Build>(Arg.Any<string>())
            .Throws(new InvalidOperationException("Deserialization failed"));

        serviceProvider.GetService(typeof(ISnakeJsonDeserializer)).Returns(snakeJsonDeserializer);

        var buildValidator = Substitute.For<IValidator<Build>>();
        serviceProvider.GetService(typeof(IValidator<Build>)).Returns(buildValidator);

        // Act
        Func<Task> act = () => _sut.Assert(serviceProvider, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Deserialization failed")
            .ConfigureAwait(true);

        Received.InOrder(() =>
        {
            serviceProvider.GetService(typeof(IBuildFileProvider));
            serviceProvider.GetService(typeof(ISnakeJsonDeserializer));
            serviceProvider.GetService(typeof(IValidator<Build>));

            buildFileProvider.GetContentAsync(cancellationToken);
            snakeJsonDeserializer.Deserialize<Build>("build.json");
        });

        await buildValidator
            .DidNotReceiveWithAnyArgs()
            .ValidateAsync(Arg.Any<IValidationContext>(), cancellationToken)
                .ConfigureAwait(true);
    }

    [Fact]
    public async Task Given_Check_When_ValidationFails_Then_ThrowException()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var serviceProvider = Substitute.For<IServiceProvider>();

        var buildFileProvider = Substitute.For<IBuildFileProvider>();

        buildFileProvider
            .GetContentAsync(cancellationToken)
            .Returns("build.json");

        serviceProvider.GetService(typeof(IBuildFileProvider)).Returns(buildFileProvider);

        var snakeJsonDeserializer = Substitute.For<ISnakeJsonDeserializer>();

        var build = new Build
        {
            Id = "1",
            Number = "1.0.0",
            Branch = "master",
            Commit = "abc123",
            Url = "http://localhost"
        };

        snakeJsonDeserializer
            .Deserialize<Build>("build.json")
            .Returns(build);

        serviceProvider.GetService(typeof(ISnakeJsonDeserializer)).Returns(snakeJsonDeserializer);

        var buildValidator = new InlineValidator<Build>();
        buildValidator.RuleFor(x => x.Url)
            .Custom((x, context) => context.AddFailure("Invalid Url"));

        serviceProvider.GetService(typeof(IValidator<Build>)).Returns(buildValidator);

        // Act
        Func<Task> act = () => _sut.Assert(serviceProvider, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<ValidationException>()
            .ConfigureAwait(true);

        Received.InOrder(() =>
        {
            serviceProvider.GetService(typeof(IBuildFileProvider));
            serviceProvider.GetService(typeof(ISnakeJsonDeserializer));
            serviceProvider.GetService(typeof(IValidator<Build>));

            buildFileProvider.GetContentAsync(cancellationToken);
            snakeJsonDeserializer.Deserialize<Build>("build.json");
        });
    }
}

