using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.GetOrderPage;

/// <summary>
/// Handles paginated order list queries.
/// </summary>
public sealed class GetOrderPageQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrderPageQueryHandler> logger) : IQueryHandler<GetOrderPageQuery, IReadOnlyCollection<OrderResponse>>
{
    /// <summary>
    /// Reads a page of orders and maps it to order response models.
    /// </summary>
    /// <param name="request">The order page query.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A successful result containing the requested page.</returns>
    public async Task<Result<IReadOnlyCollection<OrderResponse>>> Handle(
        GetOrderPageQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetPageAsync(
            request.Page,
            request.PageSize,
            request.MinOrderPrice,
            request.MaxOrderPrice,
            request.SortByOrderPrice,
            request.SortDescending,
            cancellationToken);

        var response = orders.Select(OrderMapper.ToResponse).ToList();

        logger.LogDebug("Read order page {Page} with {OrderCount} orders", request.Page, response.Count);

        return Result.Success<IReadOnlyCollection<OrderResponse>>(response);
    }
}
