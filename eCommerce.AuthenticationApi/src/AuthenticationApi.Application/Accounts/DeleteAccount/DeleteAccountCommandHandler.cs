using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Application.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Messages.Users;

namespace AuthenticationApi.Application.Accounts.DeleteAccount;

/// <summary>
/// Handles account deletion across the local auth store, Keycloak, and the user profile store.
/// </summary>
/// <param name="accountRepository">The repository that loads and deletes the local account.</param>
/// <param name="unitOfWork">The unit of work that commits local account deletion.</param>
/// <param name="identityProvider">The Keycloak boundary that deletes the identity.</param>
/// <param name="userProfileClient">The UserApi client that deletes the linked profile.</param>
/// <param name="cacheService">The cache used to invalidate administrator account pages.</param>
/// <param name="logger">The logger that records failures and completion.</param>
/// <remarks>UserApi deletion, local deletion, and Keycloak deletion execute in that order without one transaction.</remarks>
public sealed class DeleteAccountCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IIdentityProvider identityProvider,
    IRequestClient<DeleteUserProfileRequest> userProfileClient,
    ICacheService cacheService,
    ILogger<DeleteAccountCommandHandler> logger) : ICommandHandler<DeleteAccountCommand>
{
    /// <summary>
    /// Deletes a linked profile, local account, and Keycloak identity.
    /// </summary>
    /// <param name="request">The local account identifier.</param>
    /// <param name="cancellationToken">The token that cancels lookup, messaging, persistence, cache, and Keycloak operations.</param>
    /// <returns>
    /// A failure for a missing account, absent profile link, or rejected profile deletion. After local deletion,
    /// Keycloak deletion failure is logged but the handler still returns success.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>A local persistence failure after UserApi deletion can leave the account linked to a missing profile.</remarks>
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
        await AccountCacheKeys.InvalidatePagesAsync(cacheService, cancellationToken);

        var identityDeletion = await identityProvider.DeleteAsync(account.IdentityId, cancellationToken);

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
