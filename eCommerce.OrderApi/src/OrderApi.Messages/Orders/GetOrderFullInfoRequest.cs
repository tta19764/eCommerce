namespace OrderApi.Messages.Orders;

/// <summary>
/// Message request for reading complete order details from OrderApi.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record GetOrderFullInfoRequest(Guid OrderId);
