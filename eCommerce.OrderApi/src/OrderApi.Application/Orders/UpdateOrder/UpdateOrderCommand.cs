using SharedLibrary.Application.Abstractions.Messaging;

namespace OrderApi.Application.Orders.UpdateOrder;

/// <summary>
/// Command for replacing the item list of a pending order.
/// </summary>
/// <param name="OrderId">The order to update.</param>
/// <param name="Items">The replacement products and quantities.</param>
public sealed record UpdateOrderCommand(
    Guid OrderId,
    IReadOnlyCollection<OrderItemRequest> Items) : ICommand;
