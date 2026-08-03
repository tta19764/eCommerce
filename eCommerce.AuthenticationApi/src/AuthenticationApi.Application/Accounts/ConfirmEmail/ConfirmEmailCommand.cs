using SharedLibrary.Application.Abstractions.Messaging;

namespace AuthenticationApi.Application.Accounts.ConfirmEmail;

/// <summary>
/// Command for confirming an account email address.
/// </summary>
/// <param name="AccountId">The account identifier from the confirmation link.</param>
/// <param name="Email">The email address from the confirmation link.</param>
public sealed record ConfirmEmailCommand(Guid AccountId, string Email) : ICommand;
