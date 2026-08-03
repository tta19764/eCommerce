using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.DownloadImage;

/// <summary>
/// Defines the DownloadImageQueryHandler class used by this slice.
/// </summary>
public sealed class DownloadImageQueryHandler(
    IImageRepository imageRepository,
    IImageStorage imageStorage) : IQueryHandler<DownloadImageQuery, StoredImage>
{
    /// <summary>
    /// Executes the Handle operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public async Task<Result<StoredImage>> Handle(DownloadImageQuery request, CancellationToken cancellationToken)
    {
        var image = await imageRepository.GetByIdAsync(request.ImageId, cancellationToken);

        return image is null
            ? Result.Failure<StoredImage>(ImageErrors.NotFound)
            : await imageStorage.DownloadAsync(image.StorageKey, cancellationToken);
    }
}
