using ProductApi.Domain.Products;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Domain.Reviews;

/// <summary>
/// Product review left by a user.
/// </summary>
public sealed class ProductReview : Entity
{
    private ProductReview()
    {
        Comment = string.Empty;
    }

    private ProductReview(
        Guid id,
        Guid productId,
        Guid userId,
        int rating,
        string comment,
        DateTime createdAtUtc)
        : base(id)
    {
        ProductId = productId;
        UserId = userId;
        Rating = rating;
        Comment = comment;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Reviewed product identifier.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// User that created the review.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Rating value from one to five.
    /// </summary>
    public int Rating { get; private set; }

    /// <summary>
    /// Review text.
    /// </summary>
    public string Comment { get; private set; }

    /// <summary>
    /// UTC creation time.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Creates a product review when supplied values satisfy review invariants.
    /// </summary>
    public static Result<ProductReview> Create(
        Guid productId,
        Guid userId,
        int rating,
        string comment,
        DateTime createdAtUtc)
    {
        if (productId == Guid.Empty)
        {
            return Result.Failure<ProductReview>(ProductErrors.NotFound);
        }

        if (userId == Guid.Empty)
        {
            return Result.Failure<ProductReview>(ProductErrors.InvalidReviewUser);
        }

        if (rating is < 1 or > 5)
        {
            return Result.Failure<ProductReview>(ProductErrors.InvalidReviewRating);
        }

        return new ProductReview(
            Guid.NewGuid(),
            productId,
            userId,
            rating,
            comment.Trim(),
            createdAtUtc);
    }
}
