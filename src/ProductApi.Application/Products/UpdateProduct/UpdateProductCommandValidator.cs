using FluentValidation;

namespace ProductApi.Application.Products.UpdateProduct;

/// <summary>
/// Validates product update commands.
/// </summary>
public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>
    /// Initializes validation rules for product updates.
    /// </summary>
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.ProductId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Description)
            .NotNull()
            .MaximumLength(2000);

        RuleFor(command => command.Price)
            .GreaterThan(0);

        RuleFor(command => command.CurrencyCode)
            .NotEmpty()
            .Must(BeSupportedCurrency)
            .WithMessage("Currency code is invalid.");

        RuleFor(command => command.Quantity)
            .GreaterThanOrEqualTo(0);
    }

    private static bool BeSupportedCurrency(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            return false;
        }

        // The catalog only accepts currencies supported by the shared money value object.
        return SharedLibrary.Domain.Money.Currency.All
            .Any(currency => currency.Code == currencyCode.Trim().ToUpperInvariant());
    }
}
