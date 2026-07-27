namespace ProductApi.Api.Endpoints.Products;

/// <summary>
/// Request body used to update product details.
/// </summary>
/// <param name="Name">The product display name.</param>
/// <param name="Price">The product price amount.</param>
/// <param name="CurrencyCode">The ISO currency code for the product price.</param>
/// <param name="Quantity">The available product quantity.</param>
public sealed record UpdateProductRequest(
    string Name,
    decimal Price,
    string CurrencyCode,
    int Quantity);
