using SharedLibrary.Application.Abstractions.Messaging;

namespace UserApi.Application.Users.UpdateUser;

/// <summary>
/// Command for updating a user profile.
/// </summary>
/// <param name="UserId">The user to update.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="ImageId">The optional profile image asset identifier.</param>
public sealed record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? ImageId) : ICommand;
