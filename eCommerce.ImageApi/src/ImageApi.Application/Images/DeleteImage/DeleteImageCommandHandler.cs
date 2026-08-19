using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.DeleteImage;

/// <summary>
/// Deletes image content and its metadata record.
/// </summary>
/// <param name="imageRepository">The repository that reads and deletes image metadata.</param>
/// <param name="unitOfWork">The unit of work that persists the metadata deletion.</param>
/// <param name="imageStorage">The object storage service that deletes image content.</param>
/// <param name="logger">The logger that records successful deletions.</param>
/// <remarks>Object storage is updated before the metadata database. These operations do not share a transaction.</remarks>
public sealed class DeleteImageCommandHandler(
    IImageRepository imageRepository,
    IUnitOfWork unitOfWork,
    IImageStorage imageStorage,
    ILogger<DeleteImageCommandHandler> logger) : ICommandHandler<DeleteImageCommand>
{
    /// <summary>
    /// Deletes the specified image from object storage and then deletes its metadata.
    /// </summary>
    /// <param name="request">The command that identifies the image to delete.</param>
    /// <param name="cancellationToken">The token that cancels lookup, storage, and persistence operations.</param>
    /// <returns>
    /// A successful result when both deletions complete. A failure result indicates that the image does not exist
    /// or that object storage rejected the deletion.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
    {
        var image = await imageRepository.GetByIdAsync(request.ImageId, cancellationToken);

        if (image is null)
        {
            return Result.Failure(ImageErrors.NotFound);
        }

        var deleteResult = await imageStorage.DeleteAsync(image.StorageKey, cancellationToken);

        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        imageRepository.Delete(image);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted image {ImageId}", image.Id);

        return Result.Success();
    }
}
