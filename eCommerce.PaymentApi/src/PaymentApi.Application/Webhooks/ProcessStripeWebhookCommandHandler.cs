using PaymentApi.Application.Abstractions;
using PaymentApi.Domain.Payments;
using PaymentApi.Domain.Webhooks;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace PaymentApi.Application.Webhooks;

/// <summary>
/// Authenticates and normalizes a Stripe webhook, applies it to the tracked payment aggregate, and stores
/// the Stripe event ID in the same local transaction. Aggregate domain events enter the shared outbox, so
/// downstream order state never depends on the browser redirect.
/// </summary>
public sealed class ProcessStripeWebhookCommandHandler(
    IPaymentGateway paymentGateway,
    IPaymentRepository paymentRepository,
    IWebhookReceiptRepository receiptRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ProcessStripeWebhookCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        var parsed = paymentGateway.ParseWebhook(request.Payload, request.Signature);
        if (parsed.IsFailure)
        {
            return Result.Failure(parsed.Error);
        }

        var gatewayEvent = parsed.Value;
        // Stripe provides at-least-once delivery; a recorded event is therefore a successful no-op.
        if (await receiptRepository.ExistsAsync(gatewayEvent.EventId, cancellationToken))
        {
            return Result.Success();
        }

        var payment = await paymentRepository.GetByProviderIntentIdAsync(gatewayEvent.PaymentIntentId, cancellationToken);
        if (payment is null)
        {
            return Result.Failure(PaymentErrors.NotFound);
        }

        var stateResult = payment.ApplyProviderState(
            gatewayEvent.Status,
            gatewayEvent.LatestChargeId,
            gatewayEvent.FailureReason,
            gatewayEvent.CreatedOnUtc);
        if (stateResult.IsFailure)
        {
            return stateResult;
        }

        // The tracked Payment mutation, inbox receipt, and resulting outbox record commit together.
        receiptRepository.Add(new StripeWebhookReceipt(
            Guid.NewGuid(), gatewayEvent.EventId, gatewayEvent.EventType, gatewayEvent.ObjectId, DateTime.UtcNow));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
