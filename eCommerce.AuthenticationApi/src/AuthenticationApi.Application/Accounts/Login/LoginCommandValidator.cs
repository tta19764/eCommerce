using FluentValidation;

namespace AuthenticationApi.Application.Accounts.Login;

/// <summary>
/// Validates login requests.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
        RuleFor(command => command.Password).NotEmpty();
    }
}

