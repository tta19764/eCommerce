using SharedLibrary.Application.Abstractions.Messaging;

namespace AuthenticationApi.Application.Accounts.DeleteAccount;

/// <summary>
/// Command for deleting an account and its profile when allowed.
/// </summary>
public sealed record DeleteAccountCommand(Guid AccountId) : ICommand;

