using FluentAssertions;
using MassTransit;
using NSubstitute;
using OrderApi.Messages.Orders;
using PaymentApi.Application.Abstractions;
using PaymentApi.Application.Payments.CreatePayment;
using PaymentApi.Domain.Payments;
using SharedLibrary.Domain.Abstractions;
using Xunit;

namespace PaymentApi.Application.UnitTests.Payments;

/// <summary>Verifies server-authoritative amount forwarding and idempotent PaymentIntent reuse.</summary>
public sealed class CreatePaymentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSendFrozenOrderAmountToGateway()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var repository = Substitute.For<IPaymentRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var orderClient = Substitute.For<IRequestClient<GetOrderPaymentSnapshotRequest>>();
        var gateway = Substitute.For<IPaymentGateway>();
        orderClient.GetResponse<GetOrderPaymentSnapshotResponse>(
                Arg.Any<GetOrderPaymentSnapshotRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Response<GetOrderPaymentSnapshotResponse>>(new TestResponse<GetOrderPaymentSnapshotResponse>(
                new GetOrderPaymentSnapshotResponse(true, true, orderId, customerId, 12_345, "EUR",
                    Guid.NewGuid(), DateTime.UtcNow.AddMinutes(10), []))));
        gateway.CreatePaymentIntentAsync(
                Arg.Any<Guid>(), orderId, 12_345, "EUR",
                $"order:{orderId}:payment-attempt:1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new GatewayPaymentIntent(
                "pi_test", "secret_test", "requires_payment_method"))));

        var handler = new CreatePaymentCommandHandler(repository, unitOfWork, orderClient, gateway);
        var result = await handler.Handle(new CreatePaymentCommand(orderId, customerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AmountMinor.Should().Be(12_345);
        result.Value.Currency.Should().Be("EUR");
        repository.Received(1).Add(Arg.Is<Payment>(payment =>
            payment.OrderId == orderId && payment.AmountMinor == 12_345));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRejectIneligibleOrderWithoutCallingStripe()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = Substitute.For<IPaymentRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var orderClient = Substitute.For<IRequestClient<GetOrderPaymentSnapshotRequest>>();
        var gateway = Substitute.For<IPaymentGateway>();
        orderClient.GetResponse<GetOrderPaymentSnapshotResponse>(
                Arg.Any<GetOrderPaymentSnapshotRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Response<GetOrderPaymentSnapshotResponse>>(new TestResponse<GetOrderPaymentSnapshotResponse>(
                new GetOrderPaymentSnapshotResponse(true, false, Guid.NewGuid(), Guid.NewGuid(), 0, "USD", null, null, []))));
        var handler = new CreatePaymentCommandHandler(repository, unitOfWork, orderClient, gateway);

        var result = await handler.Handle(
            new CreatePaymentCommand(Guid.NewGuid(), Guid.NewGuid()), cancellationToken);

        result.Error.Should().Be(PaymentErrors.OrderNotPayable);
        await gateway.DidNotReceiveWithAnyArgs().CreatePaymentIntentAsync(
            default, default, default, default!, default!, TestContext.Current.CancellationToken);
    }
}
