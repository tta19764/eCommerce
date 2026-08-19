using MassTransit;
using MediatR;
using UserApi.Application.Users.DeleteUser;
using UserApi.Messages.Users;

namespace UserApi.Application.Users.Messaging;

/// <summary>
/// Deletes a profile when AuthenticationApi deletes an identity.
/// </summary>
/// <param name="sender">The mediator that runs the standard user-deletion workflow.</param>
/// <remarks>Deletion can fail when the profile owns any order. The consumer returns that domain error to AuthenticationApi.</remarks>
public sealed class DeleteUserProfileConsumer(ISender sender) : IConsumer<DeleteUserProfileRequest>
{
    /// <summary>
    /// Requests profile deletion and translates the result to the service contract.
    /// </summary>
    /// <param name="context">The consume context that contains the profile identifier.</param>
    /// <returns>A task that completes after the deletion response is sent.</returns>
    /// <exception cref="OperationCanceledException">Message processing is canceled.</exception>
    public async Task Consume(ConsumeContext<DeleteUserProfileRequest> context)
    {
        var result = await sender.Send(new DeleteUserCommand(context.Message.UserId), context.CancellationToken);

        await context.RespondAsync(new DeleteUserProfileResponse(
            result.IsSuccess,
            result.IsFailure ? result.Error.Code : null,
            result.IsFailure ? result.Error.Name : null));
    }
}
