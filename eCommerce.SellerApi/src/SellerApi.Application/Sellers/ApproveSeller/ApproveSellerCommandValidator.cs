using FluentValidation;

namespace SellerApi.Application.Sellers.ApproveSeller;

/// <summary>
/// Validates identifiers required to approve a seller application.
/// </summary>
public sealed class ApproveSellerCommandValidator : AbstractValidator<ApproveSellerCommand>
{
    /// <summary>Defines validation rules for seller approval identifiers.</summary>
    public ApproveSellerCommandValidator()
    {
        RuleFor(command => command.SellerId).NotEmpty();
        RuleFor(command => command.AdminUserId).NotEmpty();
    }
}
