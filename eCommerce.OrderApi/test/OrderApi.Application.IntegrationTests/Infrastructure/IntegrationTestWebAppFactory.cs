using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OrderApi.Application.IntegrationTests.Orders;
using OrderApi.Application.Orders.Messaging;
using OrderApi.Domain.Orders;
using OrderApi.Infrastructure;
using OrderApi.Infrastructure.Repositories;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Domain.Abstractions;
using Testcontainers.PostgreSql;
using UserApi.Messages.Users;
using Xunit;

namespace OrderApi.Application.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18.1")
        .WithDatabase("eCommerceOrderApiTest")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly ServiceProvider _serviceProvider;
    private readonly Dictionary<Guid, GetProductDetailsResponse> _products = [];
    private readonly Dictionary<Guid, GetUserDetailsResponse> _users = [];

    public IntegrationTestWebAppFactory()
    {
        var productClient = Substitute.For<IRequestClient<GetProductDetailsRequest>>();
        productClient
            .GetResponse<GetProductDetailsResponse>(
                Arg.Any<GetProductDetailsRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<GetProductDetailsRequest>();
                return Task.FromResult<Response<GetProductDetailsResponse>>(
                    new TestResponse<GetProductDetailsResponse>(
                        _products.GetValueOrDefault(
                            request.ProductId,
                            new GetProductDetailsResponse(request.ProductId, string.Empty, string.Empty, 0m, "USD", 0, Guid.Empty, null, 0.0m, 0, false))));
            });

        var userClient = Substitute.For<IRequestClient<GetUserDetailsRequest>>();
        userClient
            .GetResponse<GetUserDetailsResponse>(
                Arg.Any<GetUserDetailsRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<GetUserDetailsRequest>();
                return Task.FromResult<Response<GetUserDetailsResponse>>(
                    new TestResponse<GetUserDetailsResponse>(
                        _users.GetValueOrDefault(
                            request.UserId,
                            new GetUserDetailsResponse(request.UserId, string.Empty, string.Empty, false))));
            });

        var services = new ServiceCollection();
        var cacheService = Substitute.For<ICacheService>();

        services.AddLogging();
        services.AddApplication();
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql($"{_dbContainer.GetConnectionString()};Pooling=False"));
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<OrderDbContext>());
        services.AddTransient<GetOrderFullInfoConsumer>();
        services.AddSingleton(productClient);
        services.AddSingleton(userClient);
        services.AddSingleton(cacheService);

        _serviceProvider = services.BuildServiceProvider();
    }

    public IServiceScope CreateScope()
    {
        return _serviceProvider.CreateScope();
    }

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = CreateScope();
        await scope.ServiceProvider.GetRequiredService<OrderDbContext>().Database.EnsureCreatedAsync();
    }

    public void AddProduct(Guid productId, string name, decimal price, string currencyCode = "USD", int quantity = 10)
    {
        _products[productId] = new GetProductDetailsResponse(productId, name, string.Empty, price, currencyCode, quantity, Guid.NewGuid(), null, 0.0m, 0, true);
    }

    public void AddUser(Guid userId, string fullName, string email)
    {
        _users[userId] = new GetUserDetailsResponse(userId, fullName, email, true);
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
