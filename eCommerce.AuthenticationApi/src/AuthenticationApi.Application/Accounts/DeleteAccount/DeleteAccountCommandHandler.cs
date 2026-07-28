using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Application.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Messages.Users;

namespace AuthenticationApi.Application.Accounts.DeleteAccount;

/// <summary>
/// Handles account deletion across the local auth store, Keycloak, and the user profile store.
/// </summary>
public sealed class DeleteAccountCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IIdentityProvider identityProvider,
    IRequestClient<DeleteUserProfileRequest> userProfileClient,
    ILogger<DeleteAccountCommandHandler> logger) : ICommandHandler<DeleteAccountCommand>
{
    public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(request.AccountId, cancellationToken);

        if (account is null)
        {
            return Result.Failure(AccountErrors.NotFound);
        }

        if (account.UserId is null)
        {
            logger.LogWarning("Account {AccountId} is not linked to a user profile", request.AccountId);
            return Result.Failure(AccountErrors.ProfileNotLinked);
        }

        var profile = await userProfileClient.GetResponse<DeleteUserProfileResponse>(
            new DeleteUserProfileRequest(account.UserId.Value),
            cancellationToken);

        if (!profile.Message.Deleted)
        {
            logger.LogWarning(
                "Profile deletion failed for account {AccountId}: {ErrorCode}",
                request.AccountId,
                profile.Message.ErrorCode);

            return Result.Failure(AccountErrors.ProfileDeletionFailed);
        }

        accountRepository.Delete(account);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var identityDeletion = await identityProvider.DeleteAsync(request.AccountId, cancellationToken);

        if (identityDeletion.IsFailure)
        {
            logger.LogWarning(
                "Identity deletion failed for account {AccountId}: {ErrorCode}",
                request.AccountId,
                identityDeletion.Error.Code);
        }

        logger.LogInformation("Deleted account {AccountId}", request.AccountId);

        return Result.Success();
    }
}
