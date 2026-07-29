namespace AuthenticationApi.Application.Accounts;

/// <summary>
/// Account read model with linked user profile data.
/// </summary>
/// <param name="Id">The account identifier.</param>
/// <param name="Email">The normalized account email.</param>
/// <param name="IdentityId">The external identity provider subject.</param>
/// <param name="UserId">The linked user profile identifier.</param>
/// <param name="IsActive">Indicates whether the account is active.</param>
/// <param name="CreatedAtUtc">The account creation date.</param>
/// <param name="DeletedAtUtc">The account deletion date when inactive.</param>
/// <param name="Roles">Roles assigned to the account.</param>
/// <param name="User">Linked user profile data when available.</param>
public sealed record AccountResponse(
    Guid Id,
    string Email,
    string IdentityId,
    Guid? UserId,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? DeletedAtUtc,
    IReadOnlyCollection<RoleResponse> Roles,
    AccountUserResponse? User);
