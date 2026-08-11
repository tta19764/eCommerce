using Microsoft.EntityFrameworkCore;
using PaymentApi.Domain.Payments;
using SharedLibrary.Infrastructure.Repositories;

namespace PaymentApi.Infrastructure.Repositories;

/// <summary>Loads tracked payment aggregates for command mutation and provider correlation.</summary>
public sealed class PaymentRepository(PaymentDbContext dbContext)
    : Repository<Payment, PaymentDbContext>(dbContext), IPaymentRepository
{
    /// <inheritdoc />
    public new Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(payment => payment.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(payment => payment.OrderId == orderId, cancellationToken);

    /// <inheritdoc />
    public Task<Payment?> GetByProviderIntentIdAsync(string providerIntentId, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(payment => payment.ProviderPaymentIntentId == providerIntentId, cancellationToken);
}
