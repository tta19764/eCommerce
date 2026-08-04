using SharedLibrary.Application.Abstractions.Messaging;

namespace OrderApi.Application.Orders.CreateOrder;

/// <summary>
/// Command for creating an order from product identifiers and requested quantities.
/// </summary>
/// <param name="ClientId">The client placing the order.</param>
/// <param name="Items">The products and quantities requested by the client.</param>
public sealed record CreateOrderCommand(
    Guid ClientId,
    IReadOnlyCollection<OrderItemRequest> Items) : ICommand<Guid>;
