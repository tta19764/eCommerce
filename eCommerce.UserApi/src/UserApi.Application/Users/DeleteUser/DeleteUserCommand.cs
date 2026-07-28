using SharedLibrary.Application.Abstractions.Messaging;

namespace UserApi.Application.Users.DeleteUser;

/// <summary>
/// Command for deleting a user profile.
/// </summary>
/// <param name="UserId">The user to delete.</param>
public sealed record DeleteUserCommand(Guid UserId) : ICommand;
