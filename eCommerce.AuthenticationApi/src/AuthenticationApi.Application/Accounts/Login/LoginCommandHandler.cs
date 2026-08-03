using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Accounts.Login;

/// <summary>
/// Handles account login and token creation.
/// </summary>
public sealed class LoginCommandHandler(
    IAccountRepository accountRepository,
    IIdentityProvider identityProvider) : ICommandHandler<LoginCommand, TokenResponse>
{
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
