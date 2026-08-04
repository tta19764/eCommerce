using OrderApi.Domain.Orders;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.GetSellerOrder;

/// <summary>
/// Handles seller-order detail queries.
/// </summary>
public sealed class GetSellerOrderQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetSellerOrderQuery, SellerOrderResponse>
{
    /// <inheritdoc />
    public async Task<Result<SellerOrderResponse>> Handle(
        GetSellerOrderQuery request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetBySellerOrderIdAsync(request.SellerOrderId, cancellationToken);
        var sellerOrder = order?.SellerOrders.FirstOrDefault(sellerOrder => sellerOrder.Id == request.SellerOrderId);

        return order is null || sellerOrder is null
            ? Result.Failure<SellerOrderResponse>(OrderErrors.SellerOrderNotFound)
            : OrderMapper.ToSellerOrderResponse(sellerOrder, order.Items);
    }
}
