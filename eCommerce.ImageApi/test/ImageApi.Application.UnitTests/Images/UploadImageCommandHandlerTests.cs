using FluentAssertions;
using ImageApi.Application.Abstractions;
using ImageApi.Application.Images.UploadImage;
using ImageApi.Domain.Images;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.UnitTests.Images;

public class UploadImageCommandHandlerTests
{
    private readonly IImageRepository _imageRepositoryMock = Substitute.For<IImageRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IImageStorage _imageStorageMock = Substitute.For<IImageStorage>();

    [Fact]
    public async Task Handle_Should_UploadImageAndPersistMetadata()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        using var stream = new MemoryStream([1, 2, 3]);

        _imageStorageMock.BucketName.Returns("images");
        _imageStorageMock
            .CreateStorageKey(Arg.Any<Guid>(), "profile.png")
            .Returns(callInfo => $"images/{callInfo.Arg<Guid>():N}.png");
        _imageStorageMock
            .UploadAsync(Arg.Any<string>(), stream, "image/png", cancellationToken)
            .Returns(Result.Success());
        _imageStorageMock
            .GetReadUrlAsync(Arg.Any<string>(), cancellationToken)
            .Returns(Result.Success("http://localhost:9000/images/profile.png"));
        Image? savedImage = null;
        _imageRepositoryMock
            .When(repository => repository.Add(Arg.Any<Image>()))
            .Do(callInfo => savedImage = callInfo.Arg<Image>());

        var handler = new UploadImageCommandHandler(
            _imageRepositoryMock,
            _unitOfWorkMock,
            _imageStorageMock,
            NullLogger<UploadImageCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new UploadImageCommand("profile.png", "image/png", stream.Length, stream),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Url.Should().Be("http://localhost:9000/images/profile.png");

        _imageRepositoryMock.Received(1).Add(Arg.Any<Image>());
        savedImage.Should().NotBeNull();
        savedImage!.Id.Should().Be(result.Value.Id);
        savedImage.FileName.Should().Be("profile.png");
        savedImage.ContentType.Should().Be("image/png");
        savedImage.Size.Should().Be(stream.Length);
        savedImage.BucketName.Should().Be("images");

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnUnsupportedContentType_WhenFileIsNotImage()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        using var stream = new MemoryStream([1, 2, 3]);
        var handler = new UploadImageCommandHandler(
            _imageRepositoryMock,
            _unitOfWorkMock,
            _imageStorageMock,
            NullLogger<UploadImageCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new UploadImageCommand("document.pdf", "application/pdf", stream.Length, stream),
            cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ImageErrors.UnsupportedContentType);

        await _imageStorageMock.DidNotReceive()
            .UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _imageRepositoryMock.DidNotReceive().Add(Arg.Any<Image>());
    }
}
