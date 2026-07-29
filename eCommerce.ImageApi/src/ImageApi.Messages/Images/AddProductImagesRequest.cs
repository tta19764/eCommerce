namespace ImageApi.Messages.Images;

/// <summary>
/// Request sent when a product wants to attach already-uploaded temporary image assets.
/// </summary>
/// <param name="ProductId">The product that will store the image references.</param>
/// <param name="ImageIds">The image identifiers to attach to the product.</param>
public sealed record AddProductImagesRequest(Guid ProductId, IReadOnlyCollection<Guid> TemporaryImageIds);
