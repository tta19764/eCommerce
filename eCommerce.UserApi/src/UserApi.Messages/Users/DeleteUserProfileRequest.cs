namespace UserApi.Messages.Users;

/// <summary>
/// Message request for deleting a user profile.
/// </summary>
/// <param name="UserId">The user profile identifier owned by AuthenticationApi.</param>
public sealed record DeleteUserProfileRequest(Guid UserId);
