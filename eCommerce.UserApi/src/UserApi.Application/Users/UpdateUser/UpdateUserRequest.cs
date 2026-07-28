namespace UserApi.Application.Users.UpdateUser;

/// <summary>
/// Request body for updating a user profile.
/// </summary>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="ImageId">The optional profile image asset identifier.</param>
public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    Guid? ImageId);
