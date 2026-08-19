using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.UploadImage;

/// <summary>
/// Validates an image upload, stores its content, and persists its metadata.
/// </summary>
/// <param name="imageRepository">The repository that tracks the new image metadata.</param>
/// <param name="unitOfWork">The unit of work that persists the image metadata.</param>
/// <param name="imageStorage">The object storage service that stores the image content.</param>
/// <param name="logger">The logger that records successful uploads.</param>
/// <remarks>
/// The handler accepts JPEG, PNG, WebP, and GIF content up to 10 MiB. New images have the
/// <see cref="ImageStatus.Temporary"/> status until another service attaches them to a product or user.
/// Object storage and metadata persistence do not share a transaction. The object is stored first.
/// </remarks>
public sealed class UploadImageCommandHandler(
    IImageRepository imageRepository,
    IUnitOfWork unitOfWork,
    IImageStorage imageStorage,
    ILogger<UploadImageCommandHandler> logger) : ICommandHandler<UploadImageCommand, ImageResponse>
{
    private const long MaxImageSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    ];

    /// <summary>
    /// Validates and uploads the image, then returns its metadata and read URL.
    /// </summary>
    /// <param name="request">
    /// The upload data. The size must be from 1 byte through 10 MiB, and the content type must be allowed.
    /// </param>
    /// <param name="cancellationToken">The token that cancels storage and persistence operations.</param>
    /// <returns>
    /// A successful result with the persisted image. The URL is empty if URL generation fails after persistence.
    /// A failure result contains a validation, domain, or storage error.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>
    /// A metadata persistence failure can leave an object without a database record. The cleanup job cannot find
    /// such an object because it selects images from the metadata database.
    /// </remarks>
    public async Task<Result<ImageResponse>> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        if (request.Size <= 0)
        {
            return Result.Failure<ImageResponse>(ImageErrors.EmptyFile);
        }

        if (request.Size > MaxImageSize)
        {
            return Result.Failure<ImageResponse>(ImageErrors.TooLarge);
        }

        if (!AllowedContentTypes.Contains(request.ContentType))
        {
            return Result.Failure<ImageResponse>(ImageErrors.UnsupportedContentType);
        }

        var imageId = Guid.NewGuid();
        var storageKey = imageStorage.CreateStorageKey(imageId, request.FileName);
        var imageResult = Image.Create(
            imageId,
            request.FileName,
            request.ContentType,
            request.Size,
            storageKey,
            imageStorage.BucketName);

        if (imageResult.IsFailure)
        {
            return Result.Failure<ImageResponse>(imageResult.Error);
        }

        var uploadResult = await imageStorage.UploadAsync(
            storageKey,
            request.Content,
            request.ContentType,
            cancellationToken);

        if (uploadResult.IsFailure)
        {
            return Result.Failure<ImageResponse>(uploadResult.Error);
        }

        imageRepository.Add(imageResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var urlResult = await imageStorage.GetReadUrlAsync(storageKey, cancellationToken);

        logger.LogInformation("Uploaded image {ImageId} to {StorageKey}", imageResult.Value.Id, storageKey);

        return urlResult.IsSuccess
            ? ImageMapper.ToResponse(imageResult.Value, urlResult.Value)
            : ImageMapper.ToResponse(imageResult.Value, string.Empty);
    }
}
