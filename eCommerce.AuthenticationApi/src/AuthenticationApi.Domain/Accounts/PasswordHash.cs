namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Hashed account password.
/// </summary>
/// <param name="Value">The stored password hash.</param>
public sealed record PasswordHash(string Value);

