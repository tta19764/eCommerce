using OrderApi.Domain.Orders;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.GetSellerOrders;

/// <summary>
/// Handles seller-order collection queries.
/// </summary>
public sealed class GetSellerOrdersQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetSellerOrdersQuery, PagedListResponse<SellerOrderResponse>>
{
    /// <inheritdoc />
    public async Task<Result<PagedListResponse<SellerOrderResponse>>> Handle(
        GetSellerOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize switch
        {
            < 1 => 10,
            > 100 => 100,
            _ => request.PageSize
        };

        var orders = await orderRepository.GetOrdersBySellerIdAsync(request.SellerId, page, pageSize, cancellationToken);
        var totalCount = await orderRepository.CountBySellerIdAsync(request.SellerId, cancellationToken);

        var sellerOrders = orders
            .SelectMany(order => order.SellerOrders
                .Where(sellerOrder => sellerOrder.SellerId == request.SellerId)
                .Select(sellerOrder => OrderMapper.ToSellerOrderResponse(sellerOrder, order.Items)))
            .ToArray();

        return new PagedListResponse<SellerOrderResponse>(sellerOrders, page, pageSize, totalCount);
    }
}
