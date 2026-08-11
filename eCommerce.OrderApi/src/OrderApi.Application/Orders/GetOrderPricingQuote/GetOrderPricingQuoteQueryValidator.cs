using FluentValidation;
using OrderApi.Application.Orders.Pricing;
using SharedLibrary.Domain.Money;

namespace OrderApi.Application.Orders.GetOrderPricingQuote;

/// <summary>
/// Validates bounded public basket pricing requests.
/// </summary>
public sealed class GetOrderPricingQuoteQueryValidator : AbstractValidator<GetOrderPricingQuoteQuery>
{
    /// <summary>Defines public-endpoint bounds before ProductApi or FX provider work begins.</summary>
    public GetOrderPricingQuoteQueryValidator()
    {
        RuleFor(query => query.Items)
            .NotEmpty()
            .Must(items => items.Select(item => item.ProductId).Distinct().Count() <= OrderPricingService.MaximumDistinctItems)
            .WithMessage($"A pricing quote supports at most {OrderPricingService.MaximumDistinctItems} distinct products.");

        RuleFor(query => query.CheckoutCurrency)
            .Must(code => Currency.All.Any(currency =>
                string.Equals(currency.Code, code, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Checkout currency is not supported.");

        RuleForEach(query => query.Items).ChildRules(item =>
        {
            item.RuleFor(value => value.ProductId).NotEmpty();
            item.RuleFor(value => value.Quantity)
                .InclusiveBetween(1, OrderPricingService.MaximumItemQuantity);
        });
    }
}
