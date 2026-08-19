using OrderApi.Domain.Orders;
using SharedLibrary.Application.Abstractions.Messaging;

namespace OrderApi.Application.Orders.UpdateOrderStatus;

/// <summary>
/// Requests an administrator-controlled transition for a main order.
/// </summary>
/// <param name="OrderId">The identifier of the order to update.</param>
/// <param name="Status">The requested lifecycle status. Paid is rejected because only PaymentApi can authorize it.</param>
public sealed record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : ICommand;
