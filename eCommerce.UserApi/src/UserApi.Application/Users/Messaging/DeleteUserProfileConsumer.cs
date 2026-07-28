using MassTransit;
using MediatR;
using UserApi.Application.Users.DeleteUser;
using UserApi.Domain.Users;
using UserApi.Messages.Users;

namespace UserApi.Application.Users.Messaging;

/// <summary>
/// Deletes a profile when AuthenticationApi deletes an identity.
/// </summary>
public sealed class DeleteUserProfileConsumer(
    IUserRepository userRepository,
    ISender sender) : IConsumer<DeleteUserProfileRequest>
{
    public async Task Consume(ConsumeContext<DeleteUserProfileRequest> context)
    {
        var user = await userRepository.GetByIdentityIdAsync(
            context.Message.IdentityId.ToString(),
            context.CancellationToken);

        if (user is null)
        {
            await context.RespondAsync(new DeleteUserProfileResponse(
                false,
                UserErrors.NotFound.Code,
                UserErrors.NotFound.Name));

            return;
        }

        var result = await sender.Send(new DeleteUserCommand(user.Id), context.CancellationToken);

        await context.RespondAsync(new DeleteUserProfileResponse(
            result.IsSuccess,
            result.IsFailure ? result.Error.Code : null,
            result.IsFailure ? result.Error.Name : null));
    }
}
