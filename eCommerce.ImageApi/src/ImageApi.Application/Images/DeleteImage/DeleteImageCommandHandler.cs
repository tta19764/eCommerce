using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.DeleteImage;

/// <summary>
/// Defines the DeleteImageCommandHandler class used by this slice.
/// </summary>
public sealed class DeleteImageCommandHandler(
    IImageRepository imageRepository,
    IUnitOfWork unitOfWork,
    IImageStorage imageStorage,
    ILogger<DeleteImageCommandHandler> logger) : ICommandHandler<DeleteImageCommand>
{
    /// <summary>
    /// Executes the Handle operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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
