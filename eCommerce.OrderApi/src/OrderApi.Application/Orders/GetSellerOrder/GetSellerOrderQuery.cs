using SharedLibrary.Application.Abstractions.Messaging;

namespace OrderApi.Application.Orders.GetSellerOrder;

/// <summary>
/// Query for reading one seller-order group.
/// </summary>
public sealed record GetSellerOrderQuery(Guid SellerOrderId) : IQuery<SellerOrderResponse>;
