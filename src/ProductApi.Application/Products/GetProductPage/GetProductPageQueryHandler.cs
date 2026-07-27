using Microsoft.Extensions.Logging;
using ProductApi.Application.Products;
using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Products.GetProductPage;

/// <summary>
/// Handles paginated product list queries.
/// </summary>
public sealed class GetProductPageQueryHandler(
    IProductRepository productRepository,
    ILogger<GetProductPageQueryHandler> logger)
    : IQueryHandler<GetProductPageQuery, IReadOnlyCollection<ProductResponse>>
{
    /// <summary>
    /// Reads a page of products and maps it to response models.
    /// </summary>
    /// <param name="request">The product page query.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A successful result containing the requested page of products.</returns>
    public async Task<Result<IReadOnlyCollection<ProductResponse>>> Handle(
        GetProductPageQuery request,
        CancellationToken cancellationToken)
    {
        var products = await productRepository.GetPageAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        var response = products
            .Select(ProductMapper.ToResponse)
            .ToList();

        logger.LogDebug(
            "Read product page {Page} with page size {PageSize}; returned {ProductCount} products",
            request.Page,
            request.PageSize,
            response.Count);

        return Result.Success<IReadOnlyCollection<ProductResponse>>(response);
    }
}
