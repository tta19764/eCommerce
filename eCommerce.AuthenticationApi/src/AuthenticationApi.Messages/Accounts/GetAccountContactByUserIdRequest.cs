namespace AuthenticationApi.Messages.Accounts;

/// <summary>
/// Requests account contact and confirmation state for a user profile.
/// </summary>
/// <param name="UserId">The user profile identifier linked to the account.</param>
public sealed record GetAccountContactByUserIdRequest(Guid UserId);
