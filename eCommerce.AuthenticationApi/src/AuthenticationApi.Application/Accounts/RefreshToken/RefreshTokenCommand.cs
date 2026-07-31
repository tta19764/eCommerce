using SharedLibrary.Application.Abstractions.Messaging;

namespace AuthenticationApi.Application.Accounts.RefreshToken;

/// <summary>
/// Refreshes authentication tokens.
/// </summary>
/// <param name="RefreshToken">The refresh token issued by the identity provider.</param>
public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<TokenResponse>;
