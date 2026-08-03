namespace AuthenticationApi.Messages.Accounts;

/// <summary>
/// Defines the GetAccountUserIdByIdentityIdRequest record used by this slice.
/// </summary>
/// <param name="IdentityId">The IdentityId value.</param>
public sealed record GetAccountUserIdByIdentityIdRequest(string IdentityId);
