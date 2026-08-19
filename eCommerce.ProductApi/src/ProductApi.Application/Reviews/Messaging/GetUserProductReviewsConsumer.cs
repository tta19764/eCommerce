using MassTransit;
using Microsoft.Extensions.Logging;
using ProductApi.Domain.Reviews;
using ProductApi.Messages.Products;

namespace ProductApi.Application.Reviews.Messaging;

/// <summary>
/// Responds to service-to-service requests for user review mappings.
/// </summary>
public sealed class GetUserProductReviewsConsumer(
    IProductReviewRepository productReviewRepository,
    ILogger<GetUserProductReviewsConsumer> logger) : IConsumer<GetUserProductReviewsRequest>
{
    /// <summary>
    /// Handles a user product reviews request.
    /// </summary>
    /// <param name="context">The MassTransit consume context.</param>
    /// <returns>A task that completes after the matching product-to-review mappings are sent.</returns>
    public async Task Consume(ConsumeContext<GetUserProductReviewsRequest> context)
    {
        var reviews = await productReviewRepository.GetReviewsByUserAndProductsAsync(
            context.Message.UserId,
            context.Message.ProductIds,
            context.CancellationToken);

        var items = reviews
            .Select(review => new UserProductReviewItemDto(review.ProductId, review.Id))
            .ToList();

        logger.LogDebug(
            "Found {Count} reviews for user {UserId}",
            items.Count,
            context.Message.UserId);

        await context.RespondAsync(new GetUserProductReviewsResponse(items));
    }
}
