namespace AuthenticationApi.Messages.Accounts;

/// <summary>
/// Requests the user profile identifier associated with an identity provider subject identifier.
/// </summary>
/// <param name="IdentityId">The subject identifier issued by the identity provider.</param>
public sealed record GetAccountUserIdByIdentityIdRequest(string IdentityId);

