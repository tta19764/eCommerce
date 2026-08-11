using OrderApi.Application.Orders;

namespace OrderApi.Api.Endpoints.Orders;

/// <summary>
/// Defines the CreateOwnOrderRequest record used by this slice.
/// </summary>
/// <param name="Items">The products and quantities to order.</param>
/// <param name="CheckoutCurrency">The requested ISO checkout currency.</param>
public sealed record CreateOwnOrderRequest(
    IReadOnlyCollection<OrderItemRequest> Items,
    string CheckoutCurrency = "USD");
