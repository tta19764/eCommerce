using ProductApi.Domain.Products;

namespace ProductApi.Application.Products;

/// <summary>
/// Product type option used by product forms and catalog filters.
/// </summary>
/// <param name="Value">The enum value sent back to the API.</param>
/// <param name="Label">Human-readable display label.</param>
/// <param name="Description">Short explanation of how this product type is fulfilled.</param>
public sealed record ProductTypeResponse(
    ProductType Value,
    string Label,
    string Description);
