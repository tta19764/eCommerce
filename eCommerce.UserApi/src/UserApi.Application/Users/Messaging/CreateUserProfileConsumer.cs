using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Domain.Users;
using UserApi.Messages.Users;

namespace UserApi.Application.Users.Messaging;

/// <summary>
/// Creates a profile for an identity that was created by AuthenticationApi.
/// </summary>
public sealed class CreateUserProfileConsumer(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateUserProfileConsumer> logger) : IConsumer<CreateUserProfileRequest>
{
    public async Task Consume(ConsumeContext<CreateUserProfileRequest> context)
    {
        var identityId = context.Message.IdentityId.ToString();
        var existingUser = await userRepository.GetByIdentityIdAsync(identityId, context.CancellationToken);

        if (existingUser is not null)
        {
            await context.RespondAsync(new CreateUserProfileResponse(
                existingUser.Id,
                false,
                UserErrors.AlreadyExists.Code,
                UserErrors.AlreadyExists.Name));

            return;
        }

        var userResult = User.Create(
            new FirstName(context.Message.FirstName.Trim()),
            new LastName(context.Message.LastName.Trim()),
            new Email(context.Message.Email.Trim()));

        if (userResult.IsFailure)
        {
            await context.RespondAsync(new CreateUserProfileResponse(
                Guid.Empty,
                false,
                userResult.Error.Code,
                userResult.Error.Name));

            return;
        }

        var identityResult = userResult.Value.SetIdentityId(identityId);

        if (identityResult.IsFailure)
        {
            await context.RespondAsync(new CreateUserProfileResponse(
                userResult.Value.Id,
                false,
                identityResult.Error.Code,
                identityResult.Error.Name));

            return;
        }

        userRepository.Add(userResult.Value);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Created user profile {UserId} for identity {IdentityId}",
            userResult.Value.Id,
            identityId);

        await context.RespondAsync(new CreateUserProfileResponse(
            userResult.Value.Id,
            true,
            null,
            null));
    }
}
