namespace UserApi.Application.Users;

/// <summary>
/// User profile read model.
/// </summary>
/// <param name="Id">The user identifier.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="FullName">The user's displayable full name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="ImageId">The optional profile image asset identifier.</param>
public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    Guid? ImageId);
