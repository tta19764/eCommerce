using FluentAssertions;
using PaymentApi.Domain.Payments;
using PaymentApi.Domain.Payments.Events;
using SharedLibrary.Domain.Money;
using Xunit;

namespace PaymentApi.Domain.UnitTests.Payments;

/// <summary>Verifies payment invariants and idempotent provider-success event behavior.</summary>
public sealed class PaymentTests
{
    [Fact]
    public void Create_ShouldRejectNonPositiveAmount()
    {
        Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 0, Currency.Usd, DateTime.UtcNow)
            .Error.Should().Be(PaymentErrors.InvalidAmount);
    }

    [Fact]
    public void ApplyProviderState_ShouldRaiseSuccessOnlyOnce()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 1250, Currency.Eur, DateTime.UtcNow).Value;
        payment.AttachProviderIntent("pi_test", "requires_payment_method", DateTime.UtcNow);

        payment.ApplyProviderState("succeeded", "ch_test", null, DateTime.UtcNow).IsSuccess.Should().BeTrue();
        payment.ApplyProviderState("succeeded", "ch_test", null, DateTime.UtcNow).IsSuccess.Should().BeTrue();

        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.GetDomainEvents().OfType<PaymentSucceededDomainEvent>().Should().ContainSingle();
    }

    [Fact]
    public void AttachProviderIntent_ShouldRejectDifferentIntent()
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 1250, Currency.Usd, DateTime.UtcNow).Value;
        payment.AttachProviderIntent("pi_first", "requires_payment_method", DateTime.UtcNow);

        payment.AttachProviderIntent("pi_second", "requires_payment_method", DateTime.UtcNow)
            .Error.Should().Be(PaymentErrors.ProviderIntentAlreadyAttached);
    }
}
