using ImageApi.Domain.Images;
using ImageApi.Messages.Images;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.Messaging;

/// <summary>
/// Attaches an uploaded temporary profile image before UserApi stores the image id.
/// </summary>
public sealed class AddUserImageConsumer(
    IImageRepository imageRepository,
    IUnitOfWork unitOfWork,
    ILogger<AddUserImageConsumer> logger) : IConsumer<AddUserImageRequest>
{
    /// <summary>
    /// Executes the Consume operation.
    /// </summary>
    /// <param name="context">The context value.</param>
    public async Task Consume(ConsumeContext<AddUserImageRequest> context)
    {
        var image = context.Message.TemporaryImageId == Guid.Empty
            ? null
            : await imageRepository.GetByIdAsync(context.Message.TemporaryImageId, context.CancellationToken);

        if (image is null)
        {
            logger.LogWarning(
                "Profile image attachment for user {UserId} referenced missing image {ImageId}",
                context.Message.UserId,
                context.Message.TemporaryImageId);

            await context.RespondAsync(new AddUserImageResponse(
                false,
                null,
                [context.Message.TemporaryImageId]));

            return;
        }

        image.Attach();
        imageRepository.Update(image);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Attached profile image {ImageId} for user {UserId}",
            image.Id,
            context.Message.UserId);

        await context.RespondAsync(new AddUserImageResponse(true, image.Id, []));
    }
}
