using ImageApi.Messages.Images;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Domain.Users;

namespace UserApi.Application.Users.UpdateUser;

/// <summary>
/// Handles user profile updates.
/// </summary>
/// <param name="userRepository">The repository that loads the tracked profile.</param>
/// <param name="unitOfWork">The unit of work that persists profile changes.</param>
/// <param name="imageClient">The ImageApi client that validates and attaches a supplied temporary image.</param>
/// <param name="logger">The logger that records update outcomes.</param>
/// <remarks>
/// A non-null image identifier is attached in ImageApi before local profile validation and persistence. The two
/// services do not share a transaction. A later validation or database failure can therefore leave the image in
/// attached state without updating this profile.
/// </remarks>
public sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<AddUserImageRequest> imageClient,
    ILogger<UpdateUserCommandHandler> logger) : ICommandHandler<UpdateUserCommand>
{
    /// <summary>
    /// Updates a user profile when it exists.
    /// </summary>
    /// <param name="request">The update-user command.</param>
    /// <param name="cancellationToken">The token that cancels profile lookup, image attachment, and persistence.</param>
    /// <returns>A success result, or a not-found, invalid-image, or domain validation failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>
    /// Null name values retain their current values. A null image identifier clears the current profile image;
    /// callers cannot distinguish omission from an explicit clear with the current command shape.
    /// </remarks>
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User {UserId} was not found for update", request.UserId);
            return Result.Failure(UserErrors.NotFound);
        }

        if (request.ImageId is { } imageId)
        {
            var validationResponse = await imageClient.GetResponse<AddUserImageResponse>(
                new AddUserImageRequest(request.UserId, imageId),
                cancellationToken);

            if (!validationResponse.Message.Attached)
            {
                logger.LogWarning("User {UserId} update referenced invalid image {ImageId}", request.UserId, imageId);
                return Result.Failure(UserErrors.InvalidImage);
            }

            imageId = validationResponse.Message.ImageId ?? imageId;
            request = request with { ImageId = imageId };
        }

        var updateResult = user.Update(
            request.FirstName is null ? null : new FirstName(request.FirstName.Trim()),
            request.LastName is null ? null : new LastName(request.LastName.Trim()),
            request.ImageId);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated user {UserId}", request.UserId);

        return Result.Success();
    }
}
