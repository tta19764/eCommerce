using OrderApi.Domain.Orders;
using SharedLibrary.Application.Abstractions.Messaging;

namespace OrderApi.Application.Orders.UpdateOrderStatus;

/// <summary>
/// Defines the UpdateOrderStatusCommand record used by this slice.
/// </summary>
/// <param name="OrderId">The OrderId value.</param>
/// <param name="Status">The Status value.</param>
public sealed record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : ICommand;
