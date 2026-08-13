using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OrderApi.Messages.Orders;
using PaymentApi.Application.Abstractions;
using PaymentApi.Domain.Payments;
using PaymentApi.Infrastructure;
using PaymentApi.Infrastructure.Repositories;
using SharedLibrary.Domain.Abstractions;
using Testcontainers.PostgreSql;

namespace PaymentApi.Application.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18.1")
        .WithDatabase("PaymentApiIntegrationTests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private ServiceProvider _services = null!;

    public IServiceScope CreateScope() => _services.CreateScope();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql($"{_database.GetConnectionString()};Pooling=False"));
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IWebhookReceiptRepository, WebhookReceiptRepository>();
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<PaymentDbContext>());
        services.AddSingleton(CreateOrderClient());
        services.AddSingleton(CreatePaymentGateway());

        _services = services.BuildServiceProvider();

        using var scope = CreateScope();
        await scope.ServiceProvider
            .GetRequiredService<PaymentDbContext>()
            .Database
            .EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();
        await _database.DisposeAsync();
    }

    private static IRequestClient<GetOrderPaymentSnapshotRequest> CreateOrderClient()
    {
        var orderClient = Substitute.For<IRequestClient<GetOrderPaymentSnapshotRequest>>();
        orderClient
            .GetResponse<GetOrderPaymentSnapshotResponse>(
                Arg.Any<GetOrderPaymentSnapshotRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<GetOrderPaymentSnapshotRequest>()!;
                var response = new GetOrderPaymentSnapshotResponse(
                    true,
                    true,
                    request.OrderId,
                    request.CustomerId,
                    1250,
                    "USD",
                    null,
                    DateTime.UtcNow.AddMinutes(10),
                    []);

                return Task.FromResult<Response<GetOrderPaymentSnapshotResponse>>(
                    new TestResponse<GetOrderPaymentSnapshotResponse>(response));
            });

        return orderClient;
    }

    private static IPaymentGateway CreatePaymentGateway()
    {
        var paymentGateway = Substitute.For<IPaymentGateway>();
        paymentGateway
            .CreatePaymentIntentAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<long>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.Success(new GatewayPaymentIntent(
                $"pi_{callInfo.ArgAt<Guid>(1):N}",
                "secret",
                "requires_payment_method")));

        return paymentGateway;
    }
}
