using SellerApi.Domain.Sellers;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Domain.Stores;

/// <summary>
/// Represents one customer's review of a store.
/// </summary>
public sealed class StoreReview : Entity
{
    private StoreReview()
    {
        Comment = null!;
    }

    private StoreReview(Guid id, Guid storeId, Guid customerUserId, Guid sellerOrderId, byte rating, string comment, DateTime createdOnUtc)
        : base(id)
    {
        StoreId = storeId;
        CustomerUserId = customerUserId;
        SellerOrderId = sellerOrderId;
        Rating = rating;
        Comment = comment;
        CreatedOnUtc = createdOnUtc;
    }

    /// <summary>Gets the reviewed store identifier.</summary>
    public Guid StoreId { get; private set; }

    /// <summary>Gets the UserApi identifier of the reviewer.</summary>
    public Guid CustomerUserId { get; private set; }

    /// <summary>Gets the completed seller order that permits the review.</summary>
    public Guid SellerOrderId { get; private set; }

    /// <summary>Gets the rating from 1 through 5.</summary>
    public byte Rating { get; private set; }

    /// <summary>Gets the review comment.</summary>
    public string Comment { get; private set; }

    /// <summary>Gets the time when the review was created.</summary>
    public DateTime CreatedOnUtc { get; private set; }

    /// <summary>Creates a store review.</summary>
    /// <returns>The new review, or a validation failure.</returns>
    public static Result<StoreReview> Create(
        Guid storeId,
        Guid customerUserId,
        Guid sellerOrderId,
        byte rating,
        string comment,
        DateTime createdOnUtc)
    {
        var normalizedComment = comment.Trim();

        if (storeId == Guid.Empty
            || customerUserId == Guid.Empty
            || sellerOrderId == Guid.Empty
            || rating is < 1 or > 5
            || normalizedComment.Length > 2000)
        {
            return Result.Failure<StoreReview>(SellerErrors.InvalidReview);
        }

        return Result.Success(new StoreReview(
            Guid.NewGuid(),
            storeId,
            customerUserId,
            sellerOrderId,
            rating,
            normalizedComment,
            createdOnUtc));
    }
}
