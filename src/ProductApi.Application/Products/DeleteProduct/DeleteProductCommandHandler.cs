using Microsoft.Extensions.Logging;
using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Products.DeleteProduct;

/// <summary>
/// Handles product deletion commands.
/// </summary>
public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<DeleteProductCommandHandler> logger) : ICommandHandler<DeleteProductCommand>
{
    /// <summary>
    /// Deletes a product when it exists.
    /// </summary>
    /// <param name="request">The product deletion command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A success result or not-found result.</returns>
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} was not found for deletion", request.ProductId);

            return Result.Failure(ProductErrors.NotFound);
        }

        productRepository.Delete(product);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await ProductCacheKeys.InvalidatePagesAsync(cacheService, cancellationToken);

        logger.LogInformation("Deleted product {ProductId}", request.ProductId);

        return Result.Success();
    }
}
