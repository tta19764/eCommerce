using ProductApi.Domain.Products;

namespace ProductApi.Application.Products;

internal static class ProductMapper
{
    internal static ProductResponse ToResponse(Product product, ProductStoreResponse? store = null)
    {
        return new ProductResponse(
            product.Id,
            product.Name.Value,
            product.Description.Value,
            product.Price.Amount,
            product.Price.Currency.Code,
            product.Quantity.Value,
            product.SellerId,
            store,
            product.CategoryId,
            product.ProductType.ToString(),
            product.ImageIds,
            product.DisplayImageId,
            Math.Round(product.Rating, 1, MidpointRounding.AwayFromZero),
            product.ReviewsCount);
    }
}
