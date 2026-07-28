namespace OrderApi.Messages.Orders;

/// <summary>
/// Message response indicating whether a client has orders.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="HasOrders">Indicates whether at least one order exists for the client.</param>
public sealed record HasOrdersForClientResponse(Guid ClientId, bool HasOrders);
