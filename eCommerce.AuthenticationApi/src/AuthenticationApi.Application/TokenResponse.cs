namespace AuthenticationApi.Application;

/// <summary>
/// Authentication token response.
/// </summary>
/// <param name="AccessToken">The issued JWT access token.</param>
/// <param name="ExpiresAtUtc">The UTC expiration date.</param>
public sealed record TokenResponse(string AccessToken, DateTime ExpiresAtUtc);

