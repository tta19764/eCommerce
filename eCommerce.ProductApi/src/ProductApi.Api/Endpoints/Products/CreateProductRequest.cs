using ProductApi.Domain.Products;

namespace ProductApi.Api.Endpoints.Products;

/// <summary>
/// Contains product data. ProductApi resolves seller ownership from the authenticated user.
/// </summary>
public sealed record CreateProductRequest(string Name, string Description, decimal Price, string CurrencyCode, int Quantity, Guid CategoryId, ProductType ProductType = ProductType.Physical, IReadOnlyCollection<Guid>? ImageIds = null, Guid? DisplayImageId = null);
