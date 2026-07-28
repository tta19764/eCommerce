using SharedLibrary.Application.Abstractions.Messaging;

namespace OrderApi.Application.Orders.GetOrder;

/// <summary>
/// Query for reading one order with its item details.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderDetailsResponse>;
