using SharedLibrary.Application.Abstractions.Messaging;

namespace AuthenticationApi.Application.Accounts.RegisterSeller;

/// <summary>
/// Command for registering a seller account and linked user profile.
/// </summary>
public sealed record RegisterSellerCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : ICommand<Guid>;
