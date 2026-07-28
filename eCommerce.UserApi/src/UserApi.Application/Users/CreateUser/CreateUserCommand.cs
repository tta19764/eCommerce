using SharedLibrary.Application.Abstractions.Messaging;

namespace UserApi.Application.Users.CreateUser;

/// <summary>
/// Command for creating a user profile.
/// </summary>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="Email">The user's email address.</param>
public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email) : ICommand<Guid>;
