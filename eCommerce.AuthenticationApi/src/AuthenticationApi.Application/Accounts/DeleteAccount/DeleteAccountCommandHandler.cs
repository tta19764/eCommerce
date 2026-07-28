using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Application.Abstractions;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Accounts.DeleteAccount;

/// <summary>
/// Handles local account deletion and external Keycloak identity cleanup.
/// </summary>
public sealed class DeleteAccountCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IIdentityProvider identityProvider,
    ILogger<DeleteAccountCommandHandler> logger) : ICommandHandler<DeleteAccountCommand>
{
    public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(request.AccountId, cancellationToken);

        if (account is null)
        {
            return Result.Failure(AccountErrors.NotFound);
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
