using MassTransit;
using OrderApi.Messages.Orders;
using PaymentApi.Application.Abstractions;
using PaymentApi.Domain.Payments;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace PaymentApi.Application.Payments.CreatePayment;

/// <summary>
/// Creates one Stripe PaymentIntent from an immutable OrderApi snapshot or reuses the already attached
/// intent. The request contains no browser-provided money, and the stable provider idempotency key closes
/// the gap where Stripe succeeds but the local database save must be retried.
/// </summary>
public sealed class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<GetOrderPaymentSnapshotRequest> orderClient,
    IPaymentGateway paymentGateway) : ICommandHandler<CreatePaymentCommand, CreatePaymentResponse>
{
    /// <inheritdoc />
    public async Task<Result<CreatePaymentResponse>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        // Browser reloads and command redelivery must return the same provider intent rather than
        // create another possible charge for the order.
        var existing = await paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existing is not null && !string.IsNullOrWhiteSpace(existing.ProviderPaymentIntentId))
        {
            var existingIntent = await paymentGateway.GetPaymentIntentAsync(
                existing.ProviderPaymentIntentId, cancellationToken);
            return existingIntent.IsFailure
                ? Result.Failure<CreatePaymentResponse>(existingIntent.Error)
                : Result.Success(new CreatePaymentResponse(
                    existing.Id,
                    existingIntent.Value.ClientSecret,
                    existing.Status.ToString(),
                    existing.AmountMinor,
                    existing.Currency.Code));
        }

        // OrderApi is the sole amount/currency authority. CustomerId lets OrderApi verify both
        // ownership and payment eligibility at the service boundary.
        var orderResponse = await orderClient.GetResponse<GetOrderPaymentSnapshotResponse>(
            new GetOrderPaymentSnapshotRequest(request.OrderId, request.CustomerId),
            cancellationToken);
        var snapshot = orderResponse.Message;
        if (!snapshot.Found || !snapshot.Eligible || snapshot.AmountMinor <= 0)
        {
            return Result.Failure<CreatePaymentResponse>(PaymentErrors.OrderNotPayable);
        }

        var created = Payment.Create(
            snapshot.OrderId,
            snapshot.CustomerId,
            snapshot.AmountMinor,
            Currency.FromCode(snapshot.Currency),
            DateTime.UtcNow);
        if (created.IsFailure)
        {
            return Result.Failure<CreatePaymentResponse>(created.Error);
        }

        var payment = created.Value;
        // The key uses the stable business operation rather than the new Payment ID, allowing
        // recovery if Stripe succeeds but the following local save has an uncertain outcome.
        var gatewayResult = await paymentGateway.CreatePaymentIntentAsync(
            payment.Id,
            payment.OrderId,
            payment.AmountMinor,
            payment.Currency.Code,
            $"order:{payment.OrderId}:payment-attempt:1",
            cancellationToken);
        if (gatewayResult.IsFailure)
        {
            return Result.Failure<CreatePaymentResponse>(gatewayResult.Error);
        }

        var attachResult = payment.AttachProviderIntent(
            gatewayResult.Value.Id,
            gatewayResult.Value.Status,
            DateTime.UtcNow);
        if (attachResult.IsFailure)
        {
            return Result.Failure<CreatePaymentResponse>(attachResult.Error);
        }

        var stateResult = payment.ApplyProviderState(
            gatewayResult.Value.Status, null, null, DateTime.UtcNow);
        if (stateResult.IsFailure)
        {
            return Result.Failure<CreatePaymentResponse>(stateResult.Error);
        }

        paymentRepository.Add(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatePaymentResponse(
            payment.Id,
            gatewayResult.Value.ClientSecret,
            payment.Status.ToString(),
            payment.AmountMinor,
            payment.Currency.Code));
    }
}
