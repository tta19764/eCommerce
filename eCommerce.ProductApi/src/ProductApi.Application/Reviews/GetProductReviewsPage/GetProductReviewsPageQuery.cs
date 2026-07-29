using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;

namespace ProductApi.Application.Reviews.GetProductReviewsPage;

/// <summary>
/// Query for reading one page of product reviews.
/// </summary>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of reviews to return.</param>
public sealed record GetProductReviewsPageQuery(Guid ProductId, int Page = 1, int PageSize = 10)
    : IQuery<PagedListResponse<ProductReviewResponse>>;
