using Microsoft.Extensions.Logging;
using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace ProductApi.Application.Products.CreateProduct;

/// <summary>
/// Handles product creation commands.
/// </summary>
public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateProductCommandHandler> logger) : ICommandHandler<CreateProductCommand, Guid>
{
    /// <summary>
    /// Creates a product and persists it.
    /// </summary>
    /// <param name="request">The product creation command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The created product identifier when the operation succeeds.</returns>
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product with name {ProductName}", request.Name);

        var productResult = Product.Create(
            new Name(request.Name.Trim()),
            new Money(request.Price, Currency.FromCode(request.CurrencyCode.Trim().ToUpperInvariant())),
            new Quantity(request.Quantity),
            request.ImageIds);

        if (productResult.IsFailure)
        {
            logger.LogWarning(
                "Product creation failed with error {ErrorCode}",
                productResult.Error.Code);

            return Result.Failure<Guid>(productResult.Error);
        }

        productRepository.Add(productResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created product {ProductId}", productResult.Value.Id);

        return Result.Success(productResult.Value.Id);
    }
}
