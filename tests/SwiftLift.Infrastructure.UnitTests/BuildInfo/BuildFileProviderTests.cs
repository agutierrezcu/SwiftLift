using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NSubstitute.ExceptionExtensions;
using SwiftLift.Infrastructure.BuildInfo;

namespace SwiftLift.Infrastructure.UnitTests.BuildInfo;

public class BuildFileProviderTests
{
    [Fact]
    public async Task Given_ExistingBuildInfoFile_When_GetContentAsync_Then_ResultContent()
    {
        // Arrange
        var cancellation = CancellationToken.None;

        var buildFilePathResolver = Substitute.For<IBuildFilePathResolver>();
        buildFilePathResolver.GetRelativeToContentRoot().Returns(BuildFilePathResolver.RelativePath);

        var fileInfo = Substitute.For<IFileInfo>();
        fileInfo.Exists.Returns(true);

        var contentRootFileProvider = Substitute.For<IFileProvider>();
        contentRootFileProvider.GetFileInfo(BuildFilePathResolver.RelativePath).Returns(fileInfo);

        var hostEnvironment = Substitute.For<IHostEnvironment>();
        hostEnvironment.ContentRootFileProvider.Returns(contentRootFileProvider);

        var fileReaderService = Substitute.For<IFileReaderService>();
        fileReaderService.ReadAllTextAsync(Arg.Any<string>(), cancellation).Returns("test content");

        var sut = new BuildFileProvider(buildFilePathResolver, hostEnvironment, fileReaderService);

        // Act
        var result = await sut.GetContentAsync(cancellation)
            .ConfigureAwait(true);

        // Assert
        result.Should().Be("test content");

        Received.InOrder(() =>
        {
            buildFilePathResolver.GetRelativeToContentRoot();
            _ = hostEnvironment.ContentRootFileProvider;
            contentRootFileProvider.GetFileInfo(BuildFilePathResolver.RelativePath);
            _ = fileInfo.Exists;
            _ = fileInfo.PhysicalPath;
            fileReaderService.ReadAllTextAsync(Arg.Any<string>(), cancellation);
        });

        buildFilePathResolver
            .Received(1)
            .GetRelativeToContentRoot();

        _ = hostEnvironment
            .Received(1)
            .ContentRootFileProvider;

        contentRootFileProvider
            .Received(1)
            .GetFileInfo(BuildFilePathResolver.RelativePath);

        _ = fileInfo
            .Received(1)
            .Exists;

        _ = fileInfo
            .Received(1)
            .PhysicalPath;

        await fileReaderService
            .Received(1)
            .ReadAllTextAsync(Arg.Any<string>(), cancellation)
            .ConfigureAwait(true);
    }

    [Fact]
    public async Task Given_NonExistentFilePath_When_FileInfoDoesNotExist_Then_ThrowInvalidOperationException()
    {
        // Arrange
        var cancellation = CancellationToken.None;

        var buildFilePathResolver = Substitute.For<IBuildFilePathResolver>();
        buildFilePathResolver.GetRelativeToContentRoot().Returns("/non-existent-file.json");

        var fileInfo = Substitute.For<IFileInfo>();
        fileInfo.Exists.Returns(false);

        var contentRootFileProvider = Substitute.For<IFileProvider>();
        contentRootFileProvider.GetFileInfo("/non-existent-file.json").Returns(fileInfo);

        var hostEnvironment = Substitute.For<IHostEnvironment>();
        hostEnvironment.ContentRootFileProvider.Returns(contentRootFileProvider);

        var fileReaderService = Substitute.For<IFileReaderService>();

        var buildFileProvider = new BuildFileProvider(buildFilePathResolver, hostEnvironment, fileReaderService);

        // Act
        var act = async () => await buildFileProvider.GetContentAsync(cancellation)
                    .ConfigureAwait(true);

        // Assert
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage($"File /non-existent-file.json does not exists.")
                .ConfigureAwait(true);

        Received.InOrder(() =>
        {
            buildFilePathResolver.GetRelativeToContentRoot();
            _ = hostEnvironment.ContentRootFileProvider;
            contentRootFileProvider.GetFileInfo("/non-existent-file.json");
            _ = fileInfo.Exists;
        });

        buildFilePathResolver
            .Received(1)
            .GetRelativeToContentRoot();

        _ = hostEnvironment
            .Received(1)
            .ContentRootFileProvider;

        contentRootFileProvider
            .Received(1)
            .GetFileInfo("/non-existent-file.json");

        _ = fileInfo
            .Received(1)
            .Exists;

        await fileReaderService
           .DidNotReceiveWithAnyArgs()
           .ReadAllTextAsync(Arg.Any<string>(), cancellation)
           .ConfigureAwait(true);
    }

    [Fact]
    public async Task Given_NonExistentFilePath_When_FileInfoIsNull_Then_ThrowInvalidOperationException()
    {
        // Arrange
        var cancellation = CancellationToken.None;

        var buildFilePathResolver = Substitute.For<IBuildFilePathResolver>();
        buildFilePathResolver.GetRelativeToContentRoot().Returns("/non-existent-file.json");

        IFileInfo? fileInfo = null;

        var contentRootFileProvider = Substitute.For<IFileProvider>();
        contentRootFileProvider.GetFileInfo("/non-existent-file.json").Returns(fileInfo);

        var hostEnvironment = Substitute.For<IHostEnvironment>();
        hostEnvironment.ContentRootFileProvider.Returns(contentRootFileProvider);

        var fileReaderService = Substitute.For<IFileReaderService>();

        var buildFileProvider = new BuildFileProvider(buildFilePathResolver, hostEnvironment, fileReaderService);

        // Act
        var act = async () => await buildFileProvider.GetContentAsync(cancellation)
                    .ConfigureAwait(true);

        // Assert
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage($"File /non-existent-file.json does not exists.")
                .ConfigureAwait(true);

        Received.InOrder(() =>
        {
            buildFilePathResolver.GetRelativeToContentRoot();
            _ = hostEnvironment.ContentRootFileProvider;
            contentRootFileProvider.GetFileInfo("/non-existent-file.json");
        });

        buildFilePathResolver
            .Received(1)
            .GetRelativeToContentRoot();

        _ = hostEnvironment
            .Received(1)
            .ContentRootFileProvider;

        contentRootFileProvider
            .Received(1)
            .GetFileInfo("/non-existent-file.json");

        await fileReaderService
           .DidNotReceiveWithAnyArgs()
           .ReadAllTextAsync(Arg.Any<string>(), cancellation)
           .ConfigureAwait(true);
    }

    private sealed class FileInfo : IFileInfo
    {
        public Stream CreateReadStream()
        {
            throw new NotImplementedException();
        }

        public bool Exists => true;

        public bool IsDirectory => throw new NotImplementedException();

        public DateTimeOffset LastModified => throw new NotImplementedException();

        public long Length => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string? PhysicalPath { get; set; }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Given_NullOrWhitespaceFilePath_When_GetContentAsync_Then_ThrowInvalidOperationException(string? physicalPath)
    {
        // Arrange
        var cancellation = CancellationToken.None;

        var buildFilePathResolver = Substitute.For<IBuildFilePathResolver>();
        buildFilePathResolver.GetRelativeToContentRoot().Returns(BuildFilePathResolver.RelativePath);

        var fileInfo = new FileInfo
        {
            PhysicalPath = physicalPath
        };

        var contentRootFileProvider = Substitute.For<IFileProvider>();
        contentRootFileProvider.GetFileInfo(BuildFilePathResolver.RelativePath).Returns(fileInfo);

        var hostEnvironment = Substitute.For<IHostEnvironment>();
        hostEnvironment.ContentRootFileProvider.Returns(contentRootFileProvider);

        var fileReaderService = Substitute.For<IFileReaderService>();
        fileReaderService.ReadAllTextAsync(physicalPath, cancellation).ThrowsAsync<ArgumentException>();

        var buildFileProvider = new BuildFileProvider(buildFilePathResolver, hostEnvironment, fileReaderService);

        // Act
        var act = async () => await buildFileProvider.GetContentAsync(cancellation)
                    .ConfigureAwait(true);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
                .ConfigureAwait(true);

        Received.InOrder(() =>
        {
            buildFilePathResolver.GetRelativeToContentRoot();
            _ = hostEnvironment.ContentRootFileProvider;
            contentRootFileProvider.GetFileInfo(BuildFilePathResolver.RelativePath);
            fileReaderService.ReadAllTextAsync(physicalPath, cancellation);
        });

        buildFilePathResolver
            .Received(1)
            .GetRelativeToContentRoot();

        _ = hostEnvironment
            .Received(1)
            .ContentRootFileProvider;

        contentRootFileProvider
            .Received(1)
            .GetFileInfo(BuildFilePathResolver.RelativePath);

        await fileReaderService
           .Received(1)
           .ReadAllTextAsync(physicalPath, cancellation)
           .ConfigureAwait(true);
    }
}
