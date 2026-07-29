namespace ProductApi.Application.Reviews;

/// <summary>
/// Product review read model.
/// </summary>
/// <param name="Id">The review identifier.</param>
/// <param name="ProductId">The reviewed product identifier.</param>
/// <param name="UserId">The user that created the review.</param>
/// <param name="Rating">The review rating from one to five.</param>
/// <param name="Comment">The review text.</param>
/// <param name="CreatedAtUtc">The UTC creation time.</param>
public sealed record ProductReviewResponse(
    Guid Id,
    Guid ProductId,
    Guid UserId,
    int Rating,
    string Comment,
    DateTime CreatedAtUtc);
