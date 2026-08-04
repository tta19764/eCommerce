using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Products.GetProductTypes;

/// <summary>
/// Handles product type option queries.
/// </summary>
public sealed class GetProductTypesQueryHandler
    : IQueryHandler<GetProductTypesQuery, IReadOnlyCollection<ProductTypeResponse>>
{
    /// <inheritdoc />
    public Task<Result<IReadOnlyCollection<ProductTypeResponse>>> Handle(
        GetProductTypesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ProductTypeResponse> response =
        [
            new(ProductType.Physical, "Physical", "A shipped product with stock quantity."),
            new(ProductType.DigitalDownload, "Digital download", "A downloadable digital file."),
            new(ProductType.LicenseKey, "License key", "A digital license or activation key."),
            new(ProductType.Service, "Service", "A service delivered by the seller."),
            new(ProductType.Subscription, "Subscription", "Recurring access to a product or service."),
            new(ProductType.Bundle, "Bundle", "A package that combines multiple product items.")
        ];

        return Task.FromResult(Result.Success(response));
    }
}
