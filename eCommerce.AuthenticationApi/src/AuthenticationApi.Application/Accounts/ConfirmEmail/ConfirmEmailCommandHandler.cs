using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Accounts.ConfirmEmail;

/// <summary>
/// Handles account email confirmation.
/// </summary>
public sealed class ConfirmEmailCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IIdentityProvider identityProvider,
    ICacheService cacheService) : ICommandHandler<ConfirmEmailCommand>
{
    /// <summary>
    /// Executes the Handle operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(request.AccountId, cancellationToken);

        if (account is null)
        {
            return Result.Failure(AccountErrors.NotFound);
        }

        if (!account.IsActive)
        {
            return Result.Failure(AccountErrors.NotActive);
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        if (!string.Equals(account.Email.Value, normalizedEmail, StringComparison.Ordinal))
        {
            return Result.Failure(AccountErrors.EmailMismatch);
        }

        var identityResult = await identityProvider.ConfirmEmailAsync(account.IdentityId, cancellationToken);

        if (identityResult.IsFailure)
        {
            return Result.Failure(identityResult.Error);
        }

        var confirmationResult = account.ConfirmEmail(DateTime.UtcNow);

        if (confirmationResult.IsFailure)
        {
            return confirmationResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await AccountCacheKeys.InvalidatePagesAsync(cacheService, cancellationToken);

        return Result.Success();
    }
}
