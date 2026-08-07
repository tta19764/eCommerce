using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.GetOrderPage;

/// <summary>
/// Handles paginated order list queries.
/// </summary>
public sealed class GetOrderPageQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrderPageQueryHandler> logger) : IQueryHandler<GetOrderPageQuery, PagedListResponse<OrderResponse>>
{
    /// <summary>
    /// Reads a page of orders and maps it to order response models.
    /// </summary>
    /// <param name="request">The order page query.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A successful result containing the requested page.</returns>
    public async Task<Result<PagedListResponse<OrderResponse>>> Handle(
        GetOrderPageQuery request,
        CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);

        var orders = await orderRepository.GetPageAsync(
            page,
            pageSize,
            request.MinOrderPrice,
            request.MaxOrderPrice,
            request.SortByOrderPrice,
            request.SortDescending,
            cancellationToken);
        var totalCount = await orderRepository.CountAsync(
            request.MinOrderPrice,
            request.MaxOrderPrice,
            cancellationToken);

        var items = orders.Select(order => OrderMapper.ToResponse(order)).ToList();
        var response = new PagedListResponse<OrderResponse>(items, page, pageSize, totalCount);

        logger.LogDebug("Read order page {Page} with {OrderCount} orders", page, items.Count);

        return Result.Success(response);
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => pageSize
    };
}
