using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Accounts.ConfirmEmail;

/// <summary>
/// Handles account email confirmation.
/// </summary>
/// <param name="accountRepository">The repository that loads the local account.</param>
/// <param name="unitOfWork">The unit of work that commits the local confirmation timestamp.</param>
/// <param name="identityProvider">The Keycloak boundary that marks the identity email as verified.</param>
/// <param name="cacheService">The cache used to invalidate administrator account pages.</param>
/// <remarks>Keycloak confirmation occurs before local persistence; the two updates do not share a transaction.</remarks>
public sealed class ConfirmEmailCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IIdentityProvider identityProvider,
    ICacheService cacheService) : ICommandHandler<ConfirmEmailCommand>
{
    /// <summary>
    /// Confirms an active account when the supplied email matches its normalized email.
    /// </summary>
    /// <param name="request">The local account identifier and confirmation-link email.</param>
    /// <param name="cancellationToken">The token that cancels lookup, Keycloak, persistence, and cache operations.</param>
    /// <returns>A success result, or an account, email, identity-provider, or domain-state failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>A local save failure can leave Keycloak verified while the local account remains unconfirmed.</remarks>
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
