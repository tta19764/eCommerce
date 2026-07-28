namespace UserApi.Messages.Users;

/// <summary>
/// Message response for profile deletion.
/// </summary>
/// <param name="Deleted">Indicates whether the profile was deleted.</param>
/// <param name="ErrorCode">The failure code when deletion failed.</param>
/// <param name="ErrorMessage">The failure message when deletion failed.</param>
public sealed record DeleteUserProfileResponse(
    bool Deleted,
    string? ErrorCode,
    string? ErrorMessage);

