namespace ProductApi.Messages.Products;

/// <summary>
/// Defines the AdjustProductQuantitiesRequest record used by this slice.
/// </summary>
/// <param name="Adjustments">The Adjustments value.</param>
public sealed record AdjustProductQuantitiesRequest(IReadOnlyCollection<ProductQuantityAdjustment> Adjustments);
