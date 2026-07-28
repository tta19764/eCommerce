namespace UserApi.Messages.Users;

/// <summary>
/// Message request for reading user profile details from UserApi.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public sealed record GetUserDetailsRequest(Guid UserId);
