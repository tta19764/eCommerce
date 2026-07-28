using FluentValidation;

namespace OrderApi.Application.Orders.CreateOrder;

/// <summary>
/// Validates create-order commands before the handler asks ProductApi for product snapshots.
/// </summary>
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    /// <summary>
    /// Creates validation rules for required client and item data.
    /// </summary>
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.ClientId)
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
