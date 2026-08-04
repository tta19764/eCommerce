using FluentValidation;

namespace AuthenticationApi.Application.Accounts.RegisterSeller;

/// <summary>
/// Validates seller registration requests.
/// </summary>
public sealed class RegisterSellerCommandValidator : AbstractValidator<RegisterSellerCommand>
{
    /// <summary>
    /// Initializes validation rules for seller registration.
    /// </summary>
    public RegisterSellerCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8);
        RuleFor(command => command.FirstName).NotEmpty();
        RuleFor(command => command.LastName).NotEmpty();
    }
}
