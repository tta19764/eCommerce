using ImageApi.Domain.Images;
using ImageApi.Messages.Images;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.Messaging;

/// <summary>
/// Attaches uploaded temporary images before ProductApi stores their identifiers.
/// </summary>
/// <param name="imageRepository">The repository that resolves and tracks image metadata.</param>
/// <param name="unitOfWork">The unit of work that persists lifecycle changes.</param>
/// <param name="logger">The logger that records attachment outcomes.</param>
/// <remarks>
/// The batch is atomic at the metadata level. The consumer persists no status changes if any distinct image
/// identifier is empty or missing. Repeated identifiers are processed once.
/// </remarks>
public sealed class AddProductImagesConsumer(
    IImageRepository imageRepository,
    IUnitOfWork unitOfWork,
    ILogger<AddProductImagesConsumer> logger) : IConsumer<AddProductImagesRequest>
{
    /// <summary>
    /// Validates the requested image identifiers and marks the complete batch as attached.
    /// </summary>
    /// <param name="context">The consume context that contains the product and temporary image identifiers.</param>
    /// <returns>A task that completes after the response is sent.</returns>
    /// <exception cref="OperationCanceledException">Message processing is canceled.</exception>
    public async Task Consume(ConsumeContext<AddProductImagesRequest> context)
    {
        var missingImageIds = new List<Guid>();
        var attachedImageIds = new List<Guid>();

        foreach (var imageId in context.Message.TemporaryImageIds.Distinct())
        {
            var image = imageId == Guid.Empty
                ? null
                : await imageRepository.GetByIdAsync(imageId, context.CancellationToken);

            if (image is null)
            {
                missingImageIds.Add(imageId);
                continue;
            }

            image.Attach();
            attachedImageIds.Add(image.Id);
        }

        // Do not persist a partial attachment batch. ProductApi must not store a partly valid image list.
        if (missingImageIds.Count == 0)
        {
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }

        logger.LogInformation(
            "Attached {AttachedImageCount} images for product {ProductId} with {MissingImageCount} missing",
            attachedImageIds.Count,
            context.Message.ProductId,
            missingImageIds.Count);

        await context.RespondAsync(new AddProductImagesResponse(
            missingImageIds.Count == 0,
            attachedImageIds,
            missingImageIds));
    }
}
