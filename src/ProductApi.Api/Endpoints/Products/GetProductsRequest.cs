namespace ProductApi.Api.Endpoints.Products;

using ProductApi.Domain.Products;

/// <summary>
/// Query-string parameters used to read a page of products.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of products to return.</param>
public sealed record GetProductsRequest(
    int Page = 1,
    int PageSize = 10,
    string? Query = null,
    Guid? CategoryId = null,
    bool IncludeSubcategories = true,
    ProductType? ProductType = null,
    Guid? SellerId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    decimal? MinRating = null,
    bool? InStock = null,
    ProductSortBy SortBy = ProductSortBy.Default,
    bool SortDescending = true);
