using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.GetOrder;

/// <summary>
/// Handles single-order detail queries.
/// </summary>
public sealed class GetOrderQueryHandler(
    IOrderRepository orderRepository,
    IRequestClient<GetUserProductReviewsRequest> userReviewsClient,
    ILogger<GetOrderQueryHandler> logger) : IQueryHandler<GetOrderQuery, OrderDetailsResponse>
{
    /// <summary>
    /// Reads one order with product snapshot items.
    /// </summary>
    /// <param name="request">The order detail query.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The order details, or a not-found failure.</returns>
    public async Task<Result<OrderDetailsResponse>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} was not found", request.OrderId);
            return Result.Failure<OrderDetailsResponse>(OrderErrors.NotFound);
        }

        Dictionary<Guid, Guid>? userReviews = null;

        if (order.Status == OrderStatus.Completed && order.Items.Count > 0)
        {
            var productIds = order.Items.Select(item => item.ProductId).Distinct().ToList();
            var reviewsResponse = await userReviewsClient.GetResponse<GetUserProductReviewsResponse>(
                new GetUserProductReviewsRequest(order.ClientId, productIds),
                cancellationToken);

            userReviews = reviewsResponse.Message.Reviews.ToDictionary(r => r.ProductId, r => r.ReviewId);
        }

        return Result.Success(OrderMapper.ToDetailsResponse(order, userReviews));
    }
}
