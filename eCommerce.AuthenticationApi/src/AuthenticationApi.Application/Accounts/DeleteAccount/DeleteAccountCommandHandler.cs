using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Application.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Messages.Users;

namespace AuthenticationApi.Application.Accounts.DeleteAccount;

/// <summary>
/// Handles account deletion after the user profile service confirms profile deletion is allowed.
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

        var profile = await userProfileClient.GetResponse<DeleteUserProfileResponse>(
            new DeleteUserProfileRequest(request.AccountId),
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
