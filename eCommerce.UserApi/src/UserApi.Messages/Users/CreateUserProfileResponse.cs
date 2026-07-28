namespace UserApi.Messages.Users;

/// <summary>
/// Message response for profile creation.
/// </summary>
/// <param name="UserId">The created profile identifier.</param>
/// <param name="Created">Indicates whether the profile was created.</param>
/// <param name="ErrorCode">The failure code when creation failed.</param>
/// <param name="ErrorMessage">The failure message when creation failed.</param>
public sealed record CreateUserProfileResponse(
    Guid UserId,
    bool Created,
    string? ErrorCode,
    string? ErrorMessage);

