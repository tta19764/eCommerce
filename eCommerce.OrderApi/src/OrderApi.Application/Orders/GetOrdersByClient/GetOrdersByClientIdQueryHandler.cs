using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.GetOrdersByClient;

/// <summary>
/// Handles paginated client-order queries.
/// </summary>
public sealed class GetOrdersByClientIdQueryHandler(
    IOrderRepository orderRepository,
    IRequestClient<GetUserProductReviewsRequest> userReviewsClient,
    ICacheService cacheService,
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
        await OrderCacheKeys.TrackKeyAsync(cacheService, request.CacheKey, cancellationToken);
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);

        var orders = (await orderRepository.GetOrdersByClientId(
            request.ClientId,
            page,
            pageSize,
            cancellationToken)).ToList();
        var totalCount = await orderRepository.CountByClientIdAsync(request.ClientId, cancellationToken);

        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
        Dictionary<Guid, Guid>? userReviews = null;

        if (completedOrders.Count > 0)
        {
            var productIds = completedOrders
                .SelectMany(o => o.Items)
                .Select(item => item.ProductId)
                .Distinct()
                .ToList();

            if (productIds.Count > 0)
            {
                var reviewsResponse = await userReviewsClient.GetResponse<GetUserProductReviewsResponse>(
                    new GetUserProductReviewsRequest(request.ClientId, productIds),
                    cancellationToken);

                userReviews = reviewsResponse.Message.Reviews.ToDictionary(r => r.ProductId, r => r.ReviewId);
            }
        }

        var items = orders.Select(o => OrderMapper.ToResponse(o, userReviews)).ToList();
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
