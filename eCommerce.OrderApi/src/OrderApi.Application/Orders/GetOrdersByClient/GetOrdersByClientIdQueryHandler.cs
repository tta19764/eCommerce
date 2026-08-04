using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.GetOrdersByClient;

/// <summary>
/// Handles paginated client-order queries.
/// </summary>
public sealed class GetOrdersByClientIdQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrdersByClientIdQueryHandler> logger)
    : IQueryHandler<GetOrdersByClientIdQuery, PagedListResponse<OrderResponse>>
{
    /// <summary>
    /// Reads a page of orders for a client and maps them to response models.
    /// </summary>
    /// <param name="request">The client-order query.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A successful result containing the client's requested order page.</returns>
    public async Task<Result<PagedListResponse<OrderResponse>>> Handle(
        GetOrdersByClientIdQuery request,
        CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);

        var orders = await orderRepository.GetOrdersByClientId(
            request.ClientId,
            page,
            pageSize,
            cancellationToken);
        var totalCount = await orderRepository.CountByClientIdAsync(request.ClientId, cancellationToken);

        var items = orders.Select(OrderMapper.ToResponse).ToList();
        var response = new PagedListResponse<OrderResponse>(items, page, pageSize, totalCount);

        logger.LogDebug(
            "Read order page for client {ClientId}; returned {OrderCount} orders",
            request.ClientId,
            items.Count);

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
