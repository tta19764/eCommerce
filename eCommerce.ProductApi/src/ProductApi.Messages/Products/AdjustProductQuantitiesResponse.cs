namespace ProductApi.Messages.Products;

/// <summary>
/// Response payload for product stock quantity adjustment requests.
/// </summary>
/// <param name="Adjusted">Indicates whether all requested stock adjustments succeeded.</param>
/// <param name="MissingProductIds">Product identifiers that were not found in the catalog.</param>
/// <param name="InsufficientProductIds">Product identifiers that had insufficient stock to fulfill the request.</param>
public sealed record AdjustProductQuantitiesResponse(
    bool Adjusted,
    IReadOnlyCollection<Guid> MissingProductIds,
    IReadOnlyCollection<Guid> InsufficientProductIds);

