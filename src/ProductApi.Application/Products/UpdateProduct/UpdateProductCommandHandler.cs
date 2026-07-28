using Microsoft.Extensions.Logging;
using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace ProductApi.Application.Products.UpdateProduct;

/// <summary>
/// Handles product update commands.
/// </summary>
public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProductCommandHandler> logger) : ICommandHandler<UpdateProductCommand>
{
    /// <summary>
    /// Updates product details when the product exists.
    /// </summary>
    /// <param name="request">The product update command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A success result, not-found result, or validation failure result.</returns>
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} was not found for update", request.ProductId);

            return Result.Failure(ProductErrors.NotFound);
        }

        // Product.Update enforces domain invariants before any changed values are persisted.
        var updateResult = product.Update(
            new Name(request.Name.Trim()),
            new Money(request.Price, Currency.FromCode(request.CurrencyCode.Trim().ToUpperInvariant())),
            new Quantity(request.Quantity),
            request.ImageIds);

        if (updateResult.IsFailure)
        {
            logger.LogWarning(
                "Product {ProductId} update failed with error {ErrorCode}",
                request.ProductId,
                updateResult.Error.Code);

            return updateResult;
        }

        productRepository.Update(product);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated product {ProductId}", request.ProductId);

        return Result.Success();
    }
}
