using OrderApi.Domain.Orders;
using SharedLibrary.Application.Abstractions.Messaging;

namespace OrderApi.Application.Orders.UpdateSellerOrderStatus;

/// <summary>
/// Command for updating one seller-order group status.
/// </summary>
public sealed record UpdateSellerOrderStatusCommand(Guid SellerOrderId, OrderStatus Status) : ICommand;
