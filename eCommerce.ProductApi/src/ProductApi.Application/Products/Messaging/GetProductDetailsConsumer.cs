using MassTransit;
using Microsoft.Extensions.Logging;
using ProductApi.Domain.Products;
using ProductApi.Messages.Products;

namespace ProductApi.Application.Products.Messaging;

/// <summary>
/// Responds to service-to-service requests for product details needed by other services.
/// </summary>
public sealed class GetProductDetailsConsumer(
    IProductRepository productRepository,
    ILogger<GetProductDetailsConsumer> logger) : IConsumer<GetProductDetailsRequest>
{
    /// <summary>
    /// Handles a product-details request and returns the current product data when it exists.
    /// </summary>
    /// <param name="context">The MassTransit consume context.</param>
    public async Task Consume(ConsumeContext<GetProductDetailsRequest> context)
    {
        var product = await productRepository.GetByIdAsync(context.Message.ProductId, context.CancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} was not found for details request", context.Message.ProductId);

            await context.RespondAsync(new GetProductDetailsResponse(
                context.Message.ProductId,
                string.Empty,
                string.Empty,
                0,
                string.Empty,
                0,
                null,
                0.0m,
                0,
                false));

            return;
        }

        await context.RespondAsync(new GetProductDetailsResponse(
            product.Id,
            product.Name.Value,
            product.Description.Value,
            product.Price.Amount,
            product.Price.Currency.Code,
            product.Quantity.Value,
            product.DisplayImageId,
            Math.Round(product.Rating, 1, MidpointRounding.AwayFromZero),
            product.ReviewsCount,
            true));
    }
}
