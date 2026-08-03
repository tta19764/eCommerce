using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Accounts.RefreshToken;

/// <summary>
/// Handles refresh-token exchange.
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IIdentityProvider identityProvider) : ICommandHandler<RefreshTokenCommand, TokenResponse>
{
    /// <summary>
    /// Executes the Handle operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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
