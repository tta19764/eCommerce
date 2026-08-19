using FluentValidation;

namespace SellerApi.Application.Stores.CreateStoreReview;

/// <summary>
/// Validates store review commands before purchase verification.
/// </summary>
public sealed class CreateStoreReviewCommandValidator : AbstractValidator<CreateStoreReviewCommand>
{
    /// <summary>Defines validation rules for store review data.</summary>
    public CreateStoreReviewCommandValidator()
    {
        RuleFor(command => command.StoreId).NotEmpty();
        RuleFor(command => command.CustomerUserId).NotEmpty();
        RuleFor(command => command.SellerOrderId).NotEmpty();
        RuleFor(command => command.Rating).InclusiveBetween((byte)1, (byte)5);
        RuleFor(command => command.Comment).NotNull().MaximumLength(2000);
    }
}
