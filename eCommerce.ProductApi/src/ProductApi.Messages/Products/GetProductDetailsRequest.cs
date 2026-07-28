namespace ProductApi.Messages.Products;

/// <summary>
/// Message request for reading product details from ProductApi.
/// </summary>
/// <param name="ProductId">The product identifier.</param>
public sealed record GetProductDetailsRequest(Guid ProductId);
