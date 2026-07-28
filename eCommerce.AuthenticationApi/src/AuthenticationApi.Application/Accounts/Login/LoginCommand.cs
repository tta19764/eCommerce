using SharedLibrary.Application.Abstractions.Messaging;

namespace AuthenticationApi.Application.Accounts.Login;

/// <summary>
/// Command for logging in with email and password.
/// </summary>
public sealed record LoginCommand(string Email, string Password) : ICommand<TokenResponse>;

