namespace UserApi.Messages.Users;

/// <summary>
/// Message response containing user profile data for service-to-service callers.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="FullName">The user's displayable full name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="Found">Indicates whether the user exists.</param>
public sealed record GetUserDetailsResponse(
    Guid UserId,
    string FullName,
    string Email,
    bool Found);
