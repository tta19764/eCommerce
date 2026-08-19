using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Accounts.RefreshToken;

/// <summary>
/// Handles refresh-token exchange.
/// </summary>
/// <param name="identityProvider">The Keycloak boundary that exchanges refresh tokens.</param>
/// <remarks>The handler does not load or re-check local account active or confirmation state.</remarks>
public sealed class RefreshTokenCommandHandler(
    IIdentityProvider identityProvider) : ICommandHandler<RefreshTokenCommand, TokenResponse>
{
    /// <summary>
    /// Exchanges a trimmed refresh token through Keycloak.
    /// </summary>
    /// <param name="request">The refresh token.</param>
    /// <param name="cancellationToken">The token that cancels provider exchange.</param>
    /// <returns>New tokens on success, or an invalid-credentials failure for any provider failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<TokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var tokenResult = await identityProvider.RefreshTokenAsync(
            request.RefreshToken.Trim(),
            cancellationToken);

        return tokenResult.IsSuccess
            ? Result.Success(tokenResult.Value)
            : Result.Failure<TokenResponse>(AccountErrors.InvalidCredentials);
    }
}
