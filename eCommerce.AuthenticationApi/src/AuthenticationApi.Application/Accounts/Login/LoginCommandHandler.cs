using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Accounts.Login;

/// <summary>
/// Handles account login and token creation.
/// </summary>
/// <param name="accountRepository">The repository that verifies local account state.</param>
/// <param name="identityProvider">The Keycloak boundary that validates credentials and issues tokens.</param>
/// <remarks>Provider login failures are intentionally mapped to one invalid-credentials error.</remarks>
public sealed class LoginCommandHandler(
    IAccountRepository accountRepository,
    IIdentityProvider identityProvider) : ICommandHandler<LoginCommand, TokenResponse>
{
    /// <summary>
    /// Issues tokens for an active, locally confirmed account with valid Keycloak credentials.
    /// </summary>
    /// <param name="request">The email and password credentials.</param>
    /// <param name="cancellationToken">The token that cancels account lookup and provider authentication.</param>
    /// <returns>Tokens on success; otherwise invalid-credentials or email-not-confirmed failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<TokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var account = await accountRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        // Keycloak validates credentials, but the local account still controls whether this identity is active.
        if (account is null || !account.IsActive)
        {
            return Result.Failure<TokenResponse>(AccountErrors.InvalidCredentials);
        }

        if (!account.IsEmailConfirmed)
        {
            return Result.Failure<TokenResponse>(AccountErrors.EmailNotConfirmed);
        }

        var tokenResult = await identityProvider.LoginAsync(
            request.Email.Trim(),
            request.Password,
            cancellationToken);

        return tokenResult.IsSuccess
            ? Result.Success(tokenResult.Value)
            : Result.Failure<TokenResponse>(AccountErrors.InvalidCredentials);
    }
}
