using SharedLibrary.Application.Abstractions.Messaging;

namespace ProductApi.Application.Products.DeleteProduct;

/// <summary>
/// Command for deleting an existing product.
/// </summary>
/// <param name="ProductId">The product identifier.</param>
public sealed record DeleteProductCommand(Guid ProductId) : ICommand;
