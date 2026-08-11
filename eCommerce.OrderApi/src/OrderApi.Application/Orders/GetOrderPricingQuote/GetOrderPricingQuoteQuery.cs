using SharedLibrary.Application.Abstractions.Messaging;

namespace OrderApi.Application.Orders.GetOrderPricingQuote;

/// <summary>
/// Requests a non-binding server-side basket pricing preview.
/// </summary>
public sealed record GetOrderPricingQuoteQuery(
    IReadOnlyCollection<OrderItemRequest> Items,
    string CheckoutCurrency) : IQuery<OrderPricingQuoteResponse>;

/// <summary>
/// A non-binding basket pricing preview in one checkout currency.
/// </summary>
public sealed record OrderPricingQuoteResponse(
    Guid QuoteId,
    bool IsEstimate,
    string Provider,
    string CheckoutCurrency,
    int MinorUnitDigits,
    IReadOnlyCollection<OrderPricingQuoteItemResponse> Items,
    long SubtotalMinor,
    DateTime QuotedOnUtc,
    DateTime RateEffectiveOnUtc,
    DateTime QuoteExpiresOnUtc);

/// <summary>
/// One item in a basket pricing preview.
/// </summary>
public sealed record OrderPricingQuoteItemResponse(
    Guid ProductId,
    Guid SellerId,
    string Name,
    int Quantity,
    decimal OriginalUnitPrice,
    string OriginalCurrency,
    long CheckoutUnitAmountMinor,
    long CheckoutLineTotalMinor,
    decimal ExchangeRate);
