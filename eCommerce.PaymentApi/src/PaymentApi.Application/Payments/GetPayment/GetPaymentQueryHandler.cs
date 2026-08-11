using PaymentApi.Domain.Payments;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace PaymentApi.Application.Payments.GetPayment;

/// <summary>Loads a payment projection while enforcing customer ownership inside PaymentApi.</summary>
public sealed class GetPaymentQueryHandler(IPaymentRepository paymentRepository)
    : IQueryHandler<GetPaymentQuery, PaymentResponse>
{
    /// <inheritdoc />
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
