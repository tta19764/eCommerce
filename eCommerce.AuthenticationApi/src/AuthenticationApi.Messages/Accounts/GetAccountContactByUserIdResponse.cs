namespace AuthenticationApi.Messages.Accounts;

/// <summary>
/// Account contact response used by services that need to notify account owners.
/// </summary>
/// <param name="UserId">The requested user profile identifier.</param>
/// <param name="AccountId">The account identifier when found.</param>
/// <param name="Email">The account email address when found.</param>
/// <param name="IsEmailConfirmed">Indicates whether the account email address is confirmed.</param>
/// <param name="Found">Indicates whether an account is linked to the requested user profile.</param>
public sealed record GetAccountContactByUserIdResponse(
    Guid UserId,
    Guid? AccountId,
    string Email,
    bool IsEmailConfirmed,
    bool Found);
