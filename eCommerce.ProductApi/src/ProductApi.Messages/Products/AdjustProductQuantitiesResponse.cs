namespace ProductApi.Messages.Products;

/// <summary>
/// Defines the AdjustProductQuantitiesResponse record used by this slice.
/// </summary>
/// <param name="Adjusted">The Adjusted value.</param>
/// <param name="MissingProductIds">The MissingProductIds value.</param>
/// <param name="InsufficientProductIds">The InsufficientProductIds value.</param>
public sealed record AdjustProductQuantitiesResponse(
    bool Adjusted,
    IReadOnlyCollection<Guid> MissingProductIds,
    IReadOnlyCollection<Guid> InsufficientProductIds);
