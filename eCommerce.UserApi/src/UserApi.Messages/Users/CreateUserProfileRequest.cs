namespace UserApi.Messages.Users;

/// <summary>
/// Message request for creating a user profile from the authentication service.
/// </summary>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="Email">The user's email address.</param>
public sealed record CreateUserProfileRequest(
    string FirstName,
    string LastName,
    string Email);
