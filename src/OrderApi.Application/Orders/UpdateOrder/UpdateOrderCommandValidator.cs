using FluentValidation;

namespace OrderApi.Application.Orders.UpdateOrder;

/// <summary>
/// Validates update-order commands before product snapshots are requested.
/// </summary>
public sealed class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    /// <summary>
    /// Creates validation rules for required order and item data.
    /// </summary>
    public UpdateOrderCommandValidator()
    {
        RuleFor(command => command.OrderId)
            .NotEmpty();

        RuleFor(command => command.Items)
            .NotEmpty();

        RuleForEach(command => command.Items)
            .ChildRules(item =>
            {
                item.RuleFor(orderItem => orderItem.ProductId).NotEmpty();
                item.RuleFor(orderItem => orderItem.Quantity).GreaterThan(0);
            });
    }
}
