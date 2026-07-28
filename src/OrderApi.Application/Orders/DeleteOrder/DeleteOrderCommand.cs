using SharedLibrary.Application.Abstractions.Messaging;

namespace OrderApi.Application.Orders.DeleteOrder;

/// <summary>
/// Command for deleting an order.
/// </summary>
/// <param name="OrderId">The order to delete.</param>
public sealed record DeleteOrderCommand(Guid OrderId) : ICommand;
