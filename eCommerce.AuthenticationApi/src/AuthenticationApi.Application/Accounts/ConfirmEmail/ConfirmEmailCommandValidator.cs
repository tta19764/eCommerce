using FluentValidation;

namespace AuthenticationApi.Application.Accounts.ConfirmEmail;

/// <summary>
/// Validates email confirmation requests.
/// </summary>
public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(command => command.AccountId).NotEmpty();
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
    }
}
