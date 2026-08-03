namespace ProductApi.Api.Endpoints.Products;

/// <summary>
/// Request body used to update product details.
/// </summary>
/// <param name="Name">The product display name.</param>
/// <param name="Description">The product description shown on product detail pages.</param>
/// <param name="Price">The product price amount.</param>
/// <param name="CurrencyCode">The ISO currency code for the product price.</param>
/// <param name="Quantity">The available product quantity.</param>
/// <param name="ImageIds">The image identifiers already uploaded to ImageApi.</param>
/// <param name="DisplayImageId">The image identifier selected for product cards and primary display.</param>
public sealed record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string CurrencyCode,
    int Quantity,
    IReadOnlyCollection<Guid>? ImageIds = null,
    Guid? DisplayImageId = null);
