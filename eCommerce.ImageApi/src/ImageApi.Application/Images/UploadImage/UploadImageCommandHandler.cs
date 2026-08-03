using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.UploadImage;

/// <summary>
/// Defines the UploadImageCommandHandler class used by this slice.
/// </summary>
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
    /// Executes the Handle operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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
