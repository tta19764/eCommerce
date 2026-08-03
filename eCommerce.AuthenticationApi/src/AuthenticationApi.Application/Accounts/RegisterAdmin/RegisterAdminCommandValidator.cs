using FluentValidation;

namespace AuthenticationApi.Application.Accounts.RegisterAdmin;

/// <summary>
/// Validates administrator account registration requests.
/// </summary>
public sealed class RegisterAdminCommandValidator : AbstractValidator<RegisterAdminCommand>
{
    public RegisterAdminCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8);
        RuleFor(command => command.FirstName).NotEmpty();
        RuleFor(command => command.LastName).NotEmpty();
    }
}
