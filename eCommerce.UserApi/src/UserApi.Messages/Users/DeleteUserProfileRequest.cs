namespace UserApi.Messages.Users;

/// <summary>
/// Message request for deleting a user profile.
/// </summary>
/// <param name="IdentityId">The identity identifier linked to the profile.</param>
public sealed record DeleteUserProfileRequest(Guid IdentityId);
