namespace AuthenticationApi.Messages.Accounts;

/// <summary>
/// Defines the GetAccountUserIdByIdentityIdResponse record used by this slice.
/// </summary>
/// <param name="IdentityId">The IdentityId value.</param>
/// <param name="UserId">The UserId value.</param>
/// <param name="Found">The Found value.</param>
public sealed record GetAccountUserIdByIdentityIdResponse(
    string IdentityId,
    Guid? UserId,
    bool Found);
