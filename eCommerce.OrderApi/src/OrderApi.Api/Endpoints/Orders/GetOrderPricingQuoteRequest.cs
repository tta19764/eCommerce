using OrderApi.Application.Orders;

namespace OrderApi.Api.Endpoints.Orders;

/// <summary>
/// Request for a non-binding cart pricing preview.
/// </summary>
public sealed record GetOrderPricingQuoteRequest(
    IReadOnlyCollection<OrderItemRequest> Items,
    string CheckoutCurrency = "USD");
