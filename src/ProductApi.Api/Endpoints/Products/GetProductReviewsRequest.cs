namespace ProductApi.Api.Endpoints.Products;

/// <summary>
/// Query string values for reading product reviews by page.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of reviews returned.</param>
public sealed record GetProductReviewsRequest(int Page = 1, int PageSize = 10);
