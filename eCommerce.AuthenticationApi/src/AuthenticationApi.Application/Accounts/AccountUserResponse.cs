namespace AuthenticationApi.Application.Accounts;

/// <summary>
/// Linked user profile data returned with an account read model.
/// </summary>
/// <param name="Id">The user profile identifier.</param>
/// <param name="FullName">The profile display name.</param>
/// <param name="Email">The profile email.</param>
/// <param name="Found">Indicates whether UserApi found the linked profile.</param>
public sealed record AccountUserResponse(
    Guid Id,
    string FullName,
    string Email,
    bool Found);
