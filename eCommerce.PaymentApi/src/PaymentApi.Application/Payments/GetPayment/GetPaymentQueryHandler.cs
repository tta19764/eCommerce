using PaymentApi.Domain.Payments;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace PaymentApi.Application.Payments.GetPayment;

/// <summary>Loads a payment projection while enforcing customer ownership inside PaymentApi.</summary>
/// <param name="paymentRepository">The repository that reads payment aggregates.</param>
/// <remarks>An ownership mismatch returns the same not-found error as a missing payment.</remarks>
public sealed class GetPaymentQueryHandler(IPaymentRepository paymentRepository)
    : IQueryHandler<GetPaymentQuery, PaymentResponse>
{
    /// <summary>Gets a payment when it belongs to the authenticated customer.</summary>
    /// <param name="request">The payment and authenticated customer identifiers.</param>
    /// <param name="cancellationToken">The token that cancels the repository operation.</param>
    /// <returns>A payment projection on success, or a not-found result for a missing or non-owned payment.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<PaymentResponse>> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment is null || payment.CustomerId != request.CustomerId)
        {
            return Result.Failure<PaymentResponse>(PaymentErrors.NotFound);
        }

        return Result.Success(new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.AmountMinor,
            payment.Currency.Code,
            payment.Status.ToString(),
            payment.FailureReason,
            payment.CreatedOnUtc,
            payment.UpdatedOnUtc));
    }
}
