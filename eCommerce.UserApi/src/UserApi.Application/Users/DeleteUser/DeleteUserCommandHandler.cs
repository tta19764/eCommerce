using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Messages.Orders;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Domain.Users;

namespace UserApi.Application.Users.DeleteUser;

/// <summary>
/// Handles user profile deletion.
/// </summary>
public sealed class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<HasOrdersForClientRequest> ordersClient,
    ILogger<DeleteUserCommandHandler> logger) : ICommandHandler<DeleteUserCommand>
{
    /// <summary>
    /// Deletes a user profile only when no orders exist for the user.
    /// </summary>
    /// <param name="request">The delete-user command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A success result, or a not-found/conflict failure.</returns>
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User {UserId} was not found for deletion", request.UserId);
            return Result.Failure(UserErrors.NotFound);
        }

        var response = await ordersClient.GetResponse<HasOrdersForClientResponse>(
            new HasOrdersForClientRequest(request.UserId),
            cancellationToken);

        if (response.Message.HasOrders)
        {
            logger.LogWarning("User {UserId} cannot be deleted because orders exist", request.UserId);
            return Result.Failure(UserErrors.HasOrders);
        }

        userRepository.Delete(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted user {UserId}", request.UserId);

        return Result.Success();
    }
}
