using ProductApi.Application.Products;
using SharedLibrary.Application.Abstractions.Messaging;

namespace ProductApi.Application.Products.GetProductPage;

/// <summary>
/// Query for reading one page of products.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of products to return.</param>
public sealed record GetProductPageQuery(int Page = 1, int PageSize = 10) : IQuery<IReadOnlyCollection<ProductResponse>>;
