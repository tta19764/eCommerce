namespace ImageApi.Messages.Images;

/// <summary>
/// Response returned after ImageApi attaches product images.
/// </summary>
/// <param name="Attached">True when all product image references were attached.</param>
/// <param name="ImageIds">The image identifiers that can be saved on the product.</param>
/// <param name="MissingImageIds">The image identifiers that do not exist in ImageApi.</param>
public sealed record AddProductImagesResponse(
    bool Attached,
    IReadOnlyCollection<Guid> ImageIds,
    IReadOnlyCollection<Guid> MissingImageIds);
