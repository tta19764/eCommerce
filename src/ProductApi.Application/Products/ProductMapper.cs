using ProductApi.Domain.Products;

namespace ProductApi.Application.Products;

internal static class ProductMapper
{
    internal static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse(
            product.Id,
            product.Name.Value,
            product.Description.Value,
            product.Price.Amount,
            product.Price.Currency.Code,
            product.Quantity.Value,
            product.ImageIds);
    }
}
