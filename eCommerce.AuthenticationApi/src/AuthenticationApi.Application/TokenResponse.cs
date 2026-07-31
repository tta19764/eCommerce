namespace AuthenticationApi.Application;

/// <summary>
/// Authentication token response.
/// </summary>
/// <param name="AccessToken">The issued JWT access token.</param>
/// <param name="ExpiresAtUtc">The UTC expiration date.</param>
/// <param name="RefreshToken">The refresh token used to request a new access token.</param>
/// <param name="RefreshExpiresAtUtc">The UTC refresh token expiration date.</param>
public sealed record TokenResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshExpiresAtUtc);
