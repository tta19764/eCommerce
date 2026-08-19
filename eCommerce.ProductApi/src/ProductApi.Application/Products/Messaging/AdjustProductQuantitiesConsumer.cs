using MassTransit;
using Microsoft.Extensions.Logging;
using ProductApi.Domain.Products;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Products.Messaging;

/// <summary>
/// Validates and applies product stock adjustments requested by other services.
/// </summary>
/// <remarks>
/// The consumer combines adjustments for the same product and validates the complete batch before it changes
/// any quantity. A rejected batch does not persist partial stock changes. A successful batch invalidates cached
/// catalog pages after the unit of work commits.
/// </remarks>
public sealed class AdjustProductQuantitiesConsumer(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<AdjustProductQuantitiesConsumer> logger) : IConsumer<AdjustProductQuantitiesRequest>
{
    /// <summary>
    /// Applies the requested quantity deltas and returns the result to the requester.
    /// </summary>
    /// <param name="context">
    /// The consume context. Its message contains product identifiers and signed quantity deltas.
    /// </param>
    /// <returns>A task that completes after the response is sent and any accepted changes are persisted.</returns>
    /// <remarks>
    /// The response identifies missing products separately from products that have insufficient stock. Zero net
    /// adjustments are ignored. Cancellation, persistence, cache, and response transport exceptions propagate to
    /// MassTransit so that the configured retry and error-queue policies can apply.
    /// </remarks>
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
