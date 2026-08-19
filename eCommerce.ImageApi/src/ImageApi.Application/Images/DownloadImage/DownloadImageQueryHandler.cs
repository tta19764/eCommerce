using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.DownloadImage;

/// <summary>
/// Loads image content from object storage by using its persisted storage key.
/// </summary>
/// <param name="imageRepository">The repository that resolves image metadata.</param>
/// <param name="imageStorage">The object storage service that returns image content.</param>
public sealed class DownloadImageQueryHandler(
    IImageRepository imageRepository,
    IImageStorage imageStorage) : IQueryHandler<DownloadImageQuery, StoredImage>
{
    /// <summary>
    /// Gets the content and media type for the specified image.
    /// </summary>
    /// <param name="request">The query that identifies the image.</param>
    /// <param name="cancellationToken">The token that cancels metadata and storage operations.</param>
    /// <returns>
    /// A successful result with an owned readable stream and its media type. A failure result indicates that the
    /// metadata does not exist or object storage could not return the content.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<StoredImage>> Handle(DownloadImageQuery request, CancellationToken cancellationToken)
    {
        var image = await imageRepository.GetByIdAsync(request.ImageId, cancellationToken);

        return image is null
            ? Result.Failure<StoredImage>(ImageErrors.NotFound)
            : await imageStorage.DownloadAsync(image.StorageKey, cancellationToken);
    }
}
