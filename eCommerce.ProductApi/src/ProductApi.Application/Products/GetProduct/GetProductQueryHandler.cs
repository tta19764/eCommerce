using Microsoft.Extensions.Logging;
using ProductApi.Application.Products;
using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Products.GetProduct;

/// <summary>
/// Handles single-product read queries.
/// </summary>
public sealed class GetProductQueryHandler(
    IProductRepository productRepository,
    ILogger<GetProductQueryHandler> logger)
    : IQueryHandler<GetProductQuery, ProductResponse>
{
    /// <summary>
    /// Reads a single product and maps it to a response model.
    /// </summary>
    /// <param name="request">The product lookup query.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A product response when found, otherwise a not-found result.</returns>
    public async Task<Result<ProductResponse>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} was not found", request.ProductId);

            return Result.Failure<ProductResponse>(ProductErrors.NotFound);
        }

        logger.LogDebug("Read product {ProductId}", request.ProductId);

        return Result.Success(ProductMapper.ToResponse(product));
    }
}
