using MassTransit;
using MediatR;
using UserApi.Application.Users.DeleteUser;
using UserApi.Messages.Users;

namespace UserApi.Application.Users.Messaging;

/// <summary>
/// Deletes a profile when AuthenticationApi deletes an identity.
/// </summary>
public sealed class DeleteUserProfileConsumer(ISender sender) : IConsumer<DeleteUserProfileRequest>
{
    public async Task Consume(ConsumeContext<DeleteUserProfileRequest> context)
    {
        var result = await sender.Send(new DeleteUserCommand(context.Message.UserId), context.CancellationToken);

        await context.RespondAsync(new DeleteUserProfileResponse(
            result.IsSuccess,
            result.IsFailure ? result.Error.Code : null,
            result.IsFailure ? result.Error.Name : null));
    }
}
