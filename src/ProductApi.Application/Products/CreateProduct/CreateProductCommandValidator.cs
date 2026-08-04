using FluentValidation;

namespace ProductApi.Application.Products.CreateProduct;

/// <summary>
/// Validates product creation commands.
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>
    /// Initializes validation rules for product creation.
    /// </summary>
    public CreateProductCommandValidator()
    {
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

        RuleFor(command => command.SellerId)
            .NotEmpty();

        RuleFor(command => command.CategoryId)
            .NotEmpty();

        RuleFor(command => command.ProductType)
            .IsInEnum();
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
