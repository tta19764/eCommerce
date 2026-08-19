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
/// <param name="userRepository">The repository that loads and deletes the profile.</param>
/// <param name="unitOfWork">The unit of work that persists profile deletion.</param>
/// <param name="ordersClient">The OrderApi client that checks for historical orders.</param>
/// <param name="logger">The logger that records deletion outcomes.</param>
/// <remarks>Any order owned by the user permanently blocks profile deletion in the current implementation.</remarks>
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
    /// <param name="cancellationToken">The token that cancels lookup, OrderApi messaging, and persistence.</param>
    /// <returns>A success result, or a not-found/conflict failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
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
