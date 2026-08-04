using SharedLibrary.Application.Abstractions.Messaging;

namespace ProductApi.Application.Products.GetProductTypes;

/// <summary>
/// Query for reading available product type options.
/// </summary>
public sealed record GetProductTypesQuery : IQuery<IReadOnlyCollection<ProductTypeResponse>>;
