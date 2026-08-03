namespace ProductApi.Messages.Products;

/// <summary>
/// Defines the ProductQuantityAdjustment record used by this slice.
/// </summary>
/// <param name="ProductId">The ProductId value.</param>
/// <param name="QuantityDelta">The QuantityDelta value.</param>
public sealed record ProductQuantityAdjustment(Guid ProductId, int QuantityDelta);
