namespace ProductApi.Api.Endpoints.Products;

/// <summary>
/// Query-string parameters used to read a page of products.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of products to return.</param>
public sealed record GetProductsRequest(int Page = 1, int PageSize = 10);
