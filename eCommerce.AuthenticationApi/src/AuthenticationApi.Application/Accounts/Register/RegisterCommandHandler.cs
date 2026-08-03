using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApi.Messages.Emails;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Authorization;
using SharedLibrary.Domain.Abstractions;
using UserApi.Messages.Users;

namespace AuthenticationApi.Application.Accounts.Register;

/// <summary>
/// Handles account registration in Keycloak, the local auth store, and the user profile store.
/// </summary>
public sealed class RegisterCommandHandler(
    IAccountRepository accountRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IIdentityProvider identityProvider,
    IRequestClient<CreateUserProfileRequest> userProfileClient,
    IPublishEndpoint publishEndpoint,
    ILogger<RegisterCommandHandler> logger) : ICommandHandler<RegisterCommand, Guid>
{
    /// <summary>
    /// Executes the Handle operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var existingAccount = await accountRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existingAccount is not null)
        {
            return Result.Failure<Guid>(AccountErrors.DuplicateEmail);
        }

        var accountId = Guid.NewGuid();
        var accountResult = Account.Create(
            accountId,
            new Email(normalizedEmail));

        if (accountResult.IsFailure)
        {
            return Result.Failure<Guid>(accountResult.Error);
        }

        // Keycloak owns credentials and returns the identity subject that resource services can trust in tokens.
        var identityResult = await identityProvider.RegisterAsync(
            accountId,
            request.Email.Trim(),
            request.Password,
            request.FirstName,
            request.LastName,
            cancellationToken);

        if (identityResult.IsFailure)
        {
            logger.LogWarning(
                "Identity registration failed for account {AccountId}: {ErrorCode}",
                accountId,
                identityResult.Error.Code);

            return Result.Failure<Guid>(identityResult.Error);
        }

        var identityLinkResult = accountResult.Value.SetIdentityId(identityResult.Value);

        if (identityLinkResult.IsFailure)
        {
            await identityProvider.DeleteAsync(identityResult.Value, cancellationToken);
            return Result.Failure<Guid>(identityLinkResult.Error);
        }

        var customerRole = await roleRepository.GetByNameAsync(ApplicationRoles.Customer, cancellationToken);

        if (customerRole is not null)
        {
            accountResult.Value.AssignRole(customerRole);
        }

        accountRepository.Add(accountResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // UserApi owns profile data; AuthenticationApi is the only service that starts profile creation.
        var profile = await userProfileClient.GetResponse<CreateUserProfileResponse>(
            new CreateUserProfileRequest(
                request.FirstName,
                request.LastName,
                request.Email.Trim()),
            cancellationToken);

        if (!profile.Message.Created)
        {
            accountRepository.Delete(accountResult.Value);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await identityProvider.DeleteAsync(identityResult.Value, cancellationToken);

            logger.LogWarning(
                "Profile creation failed for account {AccountId}: {ErrorCode}",
                accountId,
                profile.Message.ErrorCode);

            return Result.Failure<Guid>(AccountErrors.ProfileCreationFailed);
        }

        var profileLinkResult = accountResult.Value.SetUserId(profile.Message.UserId);

        if (profileLinkResult.IsFailure)
        {
            accountRepository.Delete(accountResult.Value);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await identityProvider.DeleteAsync(identityResult.Value, cancellationToken);

            return Result.Failure<Guid>(profileLinkResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            new SendEmailConfirmationRequest(
                accountId,
                request.Email.Trim(),
                request.FirstName,
                request.LastName),
            cancellationToken);

        logger.LogInformation("Registered account {AccountId}", accountId);

        return Result.Success(accountId);
    }
}
