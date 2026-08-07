namespace ProductApi.Messages.Products;

/// <summary>
/// Response model containing user review mappings.
/// </summary>
/// <param name="Reviews">The list of product review mappings created by the user.</param>
public sealed record GetUserProductReviewsResponse(IReadOnlyCollection<UserProductReviewItemDto> Reviews);

/// <summary>
/// Individual product review mapping DTO for service-to-service messages.
/// </summary>
/// <param name="ProductId">The reviewed product identifier.</param>
/// <param name="ReviewId">The created review identifier.</param>
public sealed record UserProductReviewItemDto(Guid ProductId, Guid ReviewId);
