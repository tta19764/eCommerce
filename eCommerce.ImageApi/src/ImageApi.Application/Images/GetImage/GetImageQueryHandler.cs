using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.GetImage;

/// <summary>
/// Gets image metadata and a URL that can read the stored content.
/// </summary>
/// <param name="imageRepository">The repository that resolves image metadata.</param>
/// <param name="imageStorage">The object storage service that creates read URLs.</param>
public sealed class GetImageQueryHandler(
    IImageRepository imageRepository,
    IImageStorage imageStorage) : IQueryHandler<GetImageQuery, ImageResponse>
{
    /// <summary>
    /// Gets metadata and a read URL for the specified image.
    /// </summary>
    /// <param name="request">The query that identifies the image.</param>
    /// <param name="cancellationToken">The token that cancels metadata and storage operations.</param>
    /// <returns>
    /// A successful result with image metadata and a public or presigned URL. A failure result indicates that the
    /// image does not exist or that object storage could not create a URL.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<ImageResponse>> Handle(GetImageQuery request, CancellationToken cancellationToken)
    {
        var image = await imageRepository.GetByIdAsync(request.ImageId, cancellationToken);

        if (image is null)
        {
            return Result.Failure<ImageResponse>(ImageErrors.NotFound);
        }

        var url = await imageStorage.GetReadUrlAsync(image.StorageKey, cancellationToken);

        return url.IsSuccess
            ? ImageMapper.ToResponse(image, url.Value)
            : Result.Failure<ImageResponse>(url.Error);
    }
}
