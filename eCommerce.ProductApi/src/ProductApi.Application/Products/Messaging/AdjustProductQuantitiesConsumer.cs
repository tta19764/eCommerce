using MassTransit;
using Microsoft.Extensions.Logging;
using ProductApi.Domain.Products;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Products.Messaging;

/// <summary>
/// Defines the AdjustProductQuantitiesConsumer class used by this slice.
/// </summary>
public sealed class AdjustProductQuantitiesConsumer(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<AdjustProductQuantitiesConsumer> logger) : IConsumer<AdjustProductQuantitiesRequest>
{
    /// <summary>
    /// Executes the Consume operation.
    /// </summary>
    /// <param name="context">The context value.</param>
    public async Task Consume(ConsumeContext<AdjustProductQuantitiesRequest> context)
    {
        var adjustments = context.Message.Adjustments
            .GroupBy(adjustment => adjustment.ProductId)
            .Select(group => new ProductQuantityAdjustment(group.Key, group.Sum(adjustment => adjustment.QuantityDelta)))
            .Where(adjustment => adjustment.QuantityDelta != 0)
            .ToArray();

        var products = new Dictionary<Guid, Product>();
        var missingProductIds = new List<Guid>();
        var insufficientProductIds = new List<Guid>();

        foreach (var adjustment in adjustments)
        {
            var product = await productRepository.GetByIdAsync(adjustment.ProductId, context.CancellationToken);

            if (product is null)
            {
                missingProductIds.Add(adjustment.ProductId);
                continue;
            }

            if (product.Quantity.Value + adjustment.QuantityDelta < 0)
            {
                insufficientProductIds.Add(adjustment.ProductId);
                continue;
            }

            products[adjustment.ProductId] = product;
        }

        if (missingProductIds.Count > 0 || insufficientProductIds.Count > 0)
        {
            logger.LogWarning(
                "Product quantity adjustment rejected. Missing products: {MissingProductIds}; insufficient products: {InsufficientProductIds}",
                string.Join(",", missingProductIds),
                string.Join(",", insufficientProductIds));

            await context.RespondAsync(new AdjustProductQuantitiesResponse(
                false,
                missingProductIds,
                insufficientProductIds));

            return;
        }

        foreach (var adjustment in adjustments)
        {
            var product = products[adjustment.ProductId];
            var result = product.AdjustQuantity(adjustment.QuantityDelta);

            if (result.IsFailure)
            {
                insufficientProductIds.Add(adjustment.ProductId);
                continue;
            }
        }

        if (insufficientProductIds.Count > 0)
        {
            await context.RespondAsync(new AdjustProductQuantitiesResponse(false, [], insufficientProductIds));
            return;
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        await ProductCacheKeys.InvalidatePagesAsync(cacheService, context.CancellationToken);

        await context.RespondAsync(new AdjustProductQuantitiesResponse(true, [], []));
    }
}
