using FluentValidation;

namespace SellerApi.Application.Sellers.RejectSeller;

/// <summary>
/// Validates identifiers and the reason required to reject a seller application.
/// </summary>
public sealed class RejectSellerCommandValidator : AbstractValidator<RejectSellerCommand>
{
    /// <summary>Defines validation rules for seller rejection identifiers and reasons.</summary>
    public RejectSellerCommandValidator()
    {
        RuleFor(command => command.SellerId).NotEmpty();
        RuleFor(command => command.AdminUserId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(1000);
    }
}
