using ImageApi.Domain.Images;
using ImageApi.Messages.Images;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.Messaging;

/// <summary>
/// Attaches uploaded temporary images before ProductApi stores the image ids.
/// </summary>
public sealed class AddProductImagesConsumer(
    IImageRepository imageRepository,
    IUnitOfWork unitOfWork,
    ILogger<AddProductImagesConsumer> logger) : IConsumer<AddProductImagesRequest>
{
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
            imageRepository.Update(image);
            attachedImageIds.Add(image.Id);
        }

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
