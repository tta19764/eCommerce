using Microsoft.Extensions.Logging;
using ProductApi.Domain.Products;
using ProductApi.Domain.Reviews;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Reviews.GetProductReviewsPage;

/// <summary>
/// Handles paginated product review queries.
/// </summary>
public sealed class GetProductReviewsPageQueryHandler(
    IProductRepository productRepository,
    IProductReviewRepository productReviewRepository,
    ILogger<GetProductReviewsPageQueryHandler> logger)
    : IQueryHandler<GetProductReviewsPageQuery, PagedListResponse<ProductReviewResponse>>
{
    /// <summary>
    /// Reads a page of reviews for a product.
    /// </summary>
    public async Task<Result<PagedListResponse<ProductReviewResponse>>> Handle(
        GetProductReviewsPageQuery request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} was not found for review page", request.ProductId);

            return Result.Failure<PagedListResponse<ProductReviewResponse>>(ProductErrors.NotFound);
        }

        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);

        var reviews = await productReviewRepository.GetPageByProductIdAsync(
            request.ProductId,
            page,
            pageSize,
            cancellationToken);
        var totalCount = await productReviewRepository.CountByProductIdAsync(
            request.ProductId,
            cancellationToken);

        var items = reviews
            .Select(ProductReviewMapper.ToResponse)
            .ToList();

        return Result.Success(new PagedListResponse<ProductReviewResponse>(
            items,
            page,
            pageSize,
            totalCount));
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => pageSize
    };
}
