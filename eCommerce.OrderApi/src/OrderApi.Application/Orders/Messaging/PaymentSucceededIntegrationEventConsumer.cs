using MassTransit;
using PaymentApi.Messages.Payments;
using OrderApi.Domain.Orders;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace OrderApi.Application.Orders.Messaging;

/// <summary>
/// Applies Stripe-verified payment success to the tracked order aggregate.
/// </summary>
public sealed class PaymentSucceededIntegrationEventConsumer(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IConsumer<PaymentSucceededIntegrationEvent>
{
    /// <summary>
    /// Matches customer, order, internal payment ID, amount, and currency before applying the paid
    /// projection. Re-delivery of the same success is an aggregate-level idempotent operation.
    /// </summary>
    public async Task Consume(ConsumeContext<PaymentSucceededIntegrationEvent> context)
    {
        var message = context.Message;
        var order = await orderRepository.GetByIdAsync(message.OrderId, context.CancellationToken);
        if (order is null || order.ClientId != message.CustomerId)
        {
            throw new InvalidOperationException($"Order {message.OrderId} was not found for payment {message.PaymentId}.");
        }

        var result = order.RecordPaymentSucceeded(
            message.PaymentId,
            message.AmountMinor,
            Currency.FromCode(message.Currency),
            message.SucceededOnUtc);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Name);
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
