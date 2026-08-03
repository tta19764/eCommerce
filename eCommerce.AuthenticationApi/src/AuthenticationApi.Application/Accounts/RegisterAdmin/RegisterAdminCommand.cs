using SharedLibrary.Application.Abstractions.Messaging;

namespace AuthenticationApi.Application.Accounts.RegisterAdmin;

/// <summary>
/// Command for registering an administrator account and user profile.
/// </summary>
public sealed record RegisterAdminCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : ICommand<Guid>;
