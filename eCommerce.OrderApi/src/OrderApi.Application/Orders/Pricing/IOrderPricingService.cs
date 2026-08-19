using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace OrderApi.Application.Orders.Pricing;

/// <summary>
/// Produces authoritative server-side basket pricing shared by previews and order creation.
/// </summary>
public interface IOrderPricingService
{
    /// <summary>
    /// Prices and validates a normalized basket in the requested checkout currency.
    /// </summary>
    /// <param name="items">The requested product identifiers and quantities. Duplicate identifiers can be combined by the implementation.</param>
    /// <param name="checkoutCurrency">The supported ISO currency code used for all checkout totals.</param>
    /// <param name="cancellationToken">The token that cancels product or exchange-rate operations.</param>
    /// <returns>
    /// A successful immutable quote, or a failure when items, quantities, products, stock, currency, exchange rates,
    /// or calculated totals are invalid.
    /// </returns>
    Task<Result<OrderPricingResult>> PriceAsync(
        IReadOnlyCollection<OrderItemRequest> items,
        string checkoutCurrency,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// One immutable server-side basket pricing result.
/// </summary>
/// <param name="QuoteId">The identifier shared by all exchange rates in this pricing operation.</param>
/// <param name="Provider">The exchange-rate provider name.</param>
/// <param name="CheckoutCurrency">The currency used for converted prices and totals.</param>
/// <param name="MinorUnitDigits">The number of decimal digits represented by one checkout-currency minor unit.</param>
/// <param name="Items">The validated and priced product lines.</param>
/// <param name="SubtotalMinor">The checked sum of all line totals in checkout-currency minor units.</param>
/// <param name="QuotedOnUtc">The UTC time at which the application requested the quote.</param>
/// <param name="RateEffectiveOnUtc">The UTC effective time reported for the provider rates.</param>
/// <param name="QuoteExpiresOnUtc">The UTC deadline after which this quote must not create a new order.</param>
public sealed record OrderPricingResult(
    Guid QuoteId,
    string Provider,
    Currency CheckoutCurrency,
    int MinorUnitDigits,
    IReadOnlyCollection<PricedOrderLine> Items,
    long SubtotalMinor,
    DateTime QuotedOnUtc,
    DateTime RateEffectiveOnUtc,
    DateTime QuoteExpiresOnUtc);

/// <summary>
/// One priced product line using both its original and checkout currencies.
/// </summary>
/// <param name="ProductId">The ProductApi product identifier.</param>
/// <param name="SellerId">The authoritative seller identifier from the product snapshot.</param>
/// <param name="Name">The product name captured for the order snapshot.</param>
/// <param name="Quantity">The normalized positive quantity.</param>
/// <param name="OriginalUnitPrice">The authoritative product unit price before conversion.</param>
/// <param name="CheckoutUnitPrice">The converted and rounded unit price.</param>
/// <param name="CheckoutLineTotalMinor">The unit price multiplied by quantity in checkout-currency minor units.</param>
/// <param name="ExchangeRate">The rate used to convert the original currency to the checkout currency.</param>
public sealed record PricedOrderLine(
    Guid ProductId,
    Guid SellerId,
    string Name,
    int Quantity,
    Money OriginalUnitPrice,
    Money CheckoutUnitPrice,
    long CheckoutLineTotalMinor,
    decimal ExchangeRate);
