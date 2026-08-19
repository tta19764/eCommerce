using FluentValidation;
using OrderApi.Domain.Orders;

namespace OrderApi.Application.Orders.UpdateOrderStatus;

/// <summary>
/// Validates identifiers and enum values for main-order status commands.
/// </summary>
public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    /// <summary>
    /// Creates rules that require a nonempty order identifier and a defined status value.
    /// </summary>
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.Status).IsInEnum();
    }
}
