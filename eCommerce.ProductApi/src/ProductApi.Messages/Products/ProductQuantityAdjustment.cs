namespace ProductApi.Messages.Products;

/// <summary>
/// Represents a stock quantity adjustment (positive for restocking, negative for reduction) for a specific product.
/// </summary>
/// <param name="ProductId">The target product identifier.</param>
/// <param name="QuantityDelta">The relative stock quantity change.</param>
public sealed record ProductQuantityAdjustment(Guid ProductId, int QuantityDelta);

