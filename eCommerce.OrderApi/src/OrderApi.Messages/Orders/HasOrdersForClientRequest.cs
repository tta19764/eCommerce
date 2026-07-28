namespace OrderApi.Messages.Orders;

/// <summary>
/// Message request for checking whether a client has orders.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
public sealed record HasOrdersForClientRequest(Guid ClientId);
