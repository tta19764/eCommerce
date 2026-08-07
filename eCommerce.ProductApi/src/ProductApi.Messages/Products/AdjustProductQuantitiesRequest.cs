namespace ProductApi.Messages.Products;

/// <summary>
/// Requests stock quantity adjustments for multiple products (e.g. upon order placement or cancellation).
/// </summary>
/// <param name="Adjustments">The collection of product quantity adjustments to perform.</param>
public sealed record AdjustProductQuantitiesRequest(IReadOnlyCollection<ProductQuantityAdjustment> Adjustments);

