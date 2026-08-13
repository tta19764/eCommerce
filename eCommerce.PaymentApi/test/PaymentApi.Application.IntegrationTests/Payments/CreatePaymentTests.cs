using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PaymentApi.Application.IntegrationTests.Infrastructure;
using PaymentApi.Application.Payments.CreatePayment;

namespace PaymentApi.Application.IntegrationTests.Payments;

public sealed class CreatePaymentTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Create_ShouldPersistAuthoritativeOrderAmount()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreatePaymentCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var payment = await DbContext.Payments
            .AsNoTracking()
            .SingleAsync(cancellationToken);

        payment.AmountMinor.Should().Be(1250);
        payment.Currency!.Code.Should().Be("USD");
    }
}
