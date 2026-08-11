namespace PaymentApi.Domain.Payments;

/// <summary>Repository contract for tracked payment aggregate lookup and idempotent correlation.</summary>
public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByProviderIntentIdAsync(string providerIntentId, CancellationToken cancellationToken = default);
    void Add(Payment payment);
}
