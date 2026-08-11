using FluentValidation;

namespace PaymentApi.Application.Payments.CreatePayment;

/// <summary>Rejects empty order/customer identifiers before any service-to-service or Stripe call.</summary>
public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    /// <summary>Defines identity requirements for payment creation.</summary>
    public CreatePaymentCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.CustomerId).NotEmpty();
    }
}
