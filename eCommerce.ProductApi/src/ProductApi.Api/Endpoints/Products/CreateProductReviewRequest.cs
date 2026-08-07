namespace ProductApi.Api.Endpoints.Products;

/// <summary>
/// Request body used to create a product review.
/// </summary>
/// <param name="Rating">The review rating from one to five.</param>
/// <param name="Comment">The review text.</param>
public sealed record CreateProductReviewRequest(
    int Rating = 0,
    string Comment = "");
