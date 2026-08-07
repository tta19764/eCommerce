namespace AuthenticationApi.Messages.Accounts;

/// <summary>
/// Response payload containing the resolved user profile identifier for an identity provider subject identifier.
/// </summary>
/// <param name="IdentityId">The queried identity provider subject identifier.</param>
/// <param name="UserId">The resolved user profile identifier, or null if no matching account was found.</param>
/// <param name="Found">Indicates whether a matching account was found.</param>
public sealed record GetAccountUserIdByIdentityIdResponse(
    string IdentityId,
    Guid? UserId,
    bool Found);

