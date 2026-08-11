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
    Task<Result<OrderPricingResult>> PriceAsync(
        IReadOnlyCollection<OrderItemRequest> items,
        string checkoutCurrency,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// One immutable server-side basket pricing result.
/// </summary>
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
public sealed record PricedOrderLine(
    Guid ProductId,
    Guid SellerId,
    string Name,
    int Quantity,
    Money OriginalUnitPrice,
    Money CheckoutUnitPrice,
    long CheckoutLineTotalMinor,
    decimal ExchangeRate);
