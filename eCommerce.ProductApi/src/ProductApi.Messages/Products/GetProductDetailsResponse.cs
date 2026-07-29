namespace ProductApi.Messages.Products;

/// <summary>
/// Message response containing product data for service-to-service callers.
/// </summary>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Name">The current product name.</param>
/// <param name="Description">The current product description.</param>
/// <param name="Price">The current product price.</param>
/// <param name="Currency">The product price currency code.</param>
/// <param name="Quantity">The current available product quantity.</param>
/// <param name="Found">Indicates whether the product exists.</param>
public sealed record GetProductDetailsResponse(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    int Quantity,
    bool Found);
