using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Domain.Products;

/// <summary>
/// Domain errors produced by product operations.
/// </summary>
public static class ProductErrors
{
    public static readonly Error NotFound = new Error("Product.NotFound", "Product was not found");
    public static readonly Error InvalidQuantity = new Error("InvalidQuantity", "Quantity cannot be negative");
    public static readonly Error InvalidPrice = new Error("InvalidPrice", "Price must be greater than zero");
    public static readonly Error InvalidImages = new Error("Product.InvalidImages", "One or more image references are invalid");
    public static readonly Error InvalidReviewRating = new Error("Product.InvalidReviewRating", "Review rating must be from one to five");
    public static readonly Error InvalidReviewUser = new Error("Product.InvalidReviewUser", "Review user identifier is invalid");
    public static readonly Error DuplicateReview = new Error("Product.DuplicateReview", "User already reviewed this product");
}
