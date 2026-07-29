using SharedLibrary.Application.Abstractions.Messaging;

namespace ProductApi.Application.Products.UpdateProduct;

/// <summary>
/// Command for updating an existing product.
/// </summary>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Name">The product display name.</param>
/// <param name="Description">The product description shown on product detail pages.</param>
/// <param name="Price">The product price amount.</param>
/// <param name="CurrencyCode">The ISO currency code for the product price.</param>
/// <param name="Quantity">The available product quantity.</param>
/// <param name="ImageIds">The image identifiers already uploaded to ImageApi.</param>
public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    string CurrencyCode,
    int Quantity,
    IReadOnlyCollection<Guid>? ImageIds = null) : ICommand;
