using ImageApi.Domain.Images;
using ImageApi.Messages.Images;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.Messaging;

/// <summary>
/// Attaches an uploaded temporary profile image before UserApi stores its identifier.
/// </summary>
/// <param name="imageRepository">The repository that resolves and tracks image metadata.</param>
/// <param name="unitOfWork">The unit of work that persists the lifecycle change.</param>
/// <param name="logger">The logger that records attachment outcomes.</param>
public sealed class AddUserImageConsumer(
    IImageRepository imageRepository,
    IUnitOfWork unitOfWork,
    ILogger<AddUserImageConsumer> logger) : IConsumer<AddUserImageRequest>
{
    /// <summary>
    /// Marks the requested image as attached and reports whether the image exists.
    /// </summary>
    /// <param name="context">The consume context that contains the user and temporary image identifiers.</param>
    /// <returns>A task that completes after the response is sent.</returns>
    /// <exception cref="OperationCanceledException">Message processing is canceled.</exception>
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
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Attached profile image {ImageId} for user {UserId}",
            image.Id,
            context.Message.UserId);

        await context.RespondAsync(new AddUserImageResponse(true, image.Id, []));
    }
}
