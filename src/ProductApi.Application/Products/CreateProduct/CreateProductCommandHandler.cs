using ImageApi.Messages.Images;
using MassTransit;
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
    IRequestClient<AddProductImagesRequest> imageClient,
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

        var imageIds = request.ImageIds?.Distinct().ToArray();

        var productResult = Product.Create(
            new Name(request.Name.Trim()),
            new Money(request.Price, Currency.FromCode(request.CurrencyCode.Trim().ToUpperInvariant())),
            new Quantity(request.Quantity),
            imageIds);

        if (productResult.IsFailure)
        {
            logger.LogWarning(
                "Product creation failed with error {ErrorCode}",
                productResult.Error.Code);

            return Result.Failure<Guid>(productResult.Error);
        }

        if (imageIds is { Length: > 0 })
        {
            var validationResponse = await imageClient.GetResponse<AddProductImagesResponse>(
                new AddProductImagesRequest(productResult.Value.Id, imageIds),
                cancellationToken);

            if (!validationResponse.Message.Attached)
            {
                logger.LogWarning(
                    "Product {ProductId} creation referenced invalid images {ImageIds}",
                    productResult.Value.Id,
                    string.Join(",", validationResponse.Message.MissingImageIds));

                return Result.Failure<Guid>(ProductErrors.InvalidImages);
            }

            imageIds = validationResponse.Message.ImageIds.ToArray();
        }

        productRepository.Add(productResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created product {ProductId}", productResult.Value.Id);

        return Result.Success(productResult.Value.Id);
    }
}
