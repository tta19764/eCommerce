namespace ProductApi.Application.Products;

/// <summary>
/// Product read model used by product queries.
/// </summary>
/// <param name="Id">The product identifier.</param>
/// <param name="Name">The product display name.</param>
/// <param name="Description">The product description shown on product detail pages.</param>
/// <param name="Price">The product price amount.</param>
/// <param name="Currency">The product price currency code.</param>
/// <param name="Quantity">The available product quantity.</param>
/// <param name="ImageIds">The product image identifiers.</param>
/// <param name="Rating">The average product rating rounded to one digit after the decimal point.</param>
/// <param name="ReviewsCount">The number of product reviews.</param>
public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    int Quantity,
    IReadOnlyCollection<Guid> ImageIds,
    decimal Rating,
    int ReviewsCount);
