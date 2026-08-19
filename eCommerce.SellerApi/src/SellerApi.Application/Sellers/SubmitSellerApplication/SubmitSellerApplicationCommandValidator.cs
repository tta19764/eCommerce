using FluentValidation;

namespace SellerApi.Application.Sellers.SubmitSellerApplication;

/// <summary>
/// Validates seller application commands before repository lookups.
/// </summary>
public sealed class SubmitSellerApplicationCommandValidator
    : AbstractValidator<SubmitSellerApplicationCommand>
{
    /// <summary>Defines validation rules for seller and proposed store data.</summary>
    public SubmitSellerApplicationCommandValidator()
    {
        RuleFor(command => command.OwnerUserId).NotEmpty();
        RuleFor(command => command.Slug)
            .Must(slug => !string.IsNullOrWhiteSpace(slug)
                && slug.Trim().Length is >= 3 and <= 80
                && slug.Trim().All(character =>
                    char.IsAsciiLetterOrDigit(character) || character == '-'));
        RuleFor(command => command.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name)
                && name.Trim().Length is >= 2 and <= 120);
        RuleFor(command => command.Description)
            .Must(description => description is not null && description.Trim().Length <= 2000);
        RuleFor(command => command.CountryCode)
            .Must(countryCode => countryCode?.Trim().Length == 2);
        RuleFor(command => command.DefaultCurrency)
            .Must(currency => currency?.Trim().Length == 3);
    }
}
