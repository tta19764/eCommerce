using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Domain.Users;

namespace UserApi.Application.Users.UpdateUser;

/// <summary>
/// Handles user profile updates.
/// </summary>
public sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateUserCommandHandler> logger) : ICommandHandler<UpdateUserCommand>
{
    /// <summary>
    /// Updates a user profile when it exists.
    /// </summary>
    /// <param name="request">The update-user command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A success result, or a not-found/validation failure.</returns>
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User {UserId} was not found for update", request.UserId);
            return Result.Failure(UserErrors.NotFound);
        }

        var updateResult = user.Update(
            new FirstName(request.FirstName.Trim()),
            new LastName(request.LastName.Trim()),
            request.ImageId);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated user {UserId}", request.UserId);

        return Result.Success();
    }
}
