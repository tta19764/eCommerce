namespace AuthenticationApi.Messages.Accounts;

/// <summary>Requests the UserApi identifier linked to an account email address.</summary>
public sealed record GetAccountUserIdByEmailRequest(string Email);

/// <summary>Returns the UserApi identifier linked to an account email address.</summary>
public sealed record GetAccountUserIdByEmailResponse(bool Found, Guid? UserId);
