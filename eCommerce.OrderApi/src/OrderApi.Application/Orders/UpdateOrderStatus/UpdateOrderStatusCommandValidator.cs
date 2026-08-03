using FluentValidation;
using OrderApi.Domain.Orders;

namespace OrderApi.Application.Orders.UpdateOrderStatus;

/// <summary>
/// Defines the UpdateOrderStatusCommandValidator class used by this slice.
/// </summary>
public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.Status).IsInEnum();
    }
}
