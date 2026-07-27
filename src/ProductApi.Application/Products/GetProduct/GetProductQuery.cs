using ProductApi.Application.Products;
using SharedLibrary.Application.Abstractions.Messaging;

namespace ProductApi.Application.Products.GetProduct;

/// <summary>
/// Query for reading a single product.
/// </summary>
/// <param name="ProductId">The product identifier.</param>
public sealed record GetProductQuery(Guid ProductId) : IQuery<ProductResponse>;
