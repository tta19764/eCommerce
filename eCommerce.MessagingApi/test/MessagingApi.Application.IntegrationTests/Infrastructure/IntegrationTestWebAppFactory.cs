using MassTransit;
using MessagingApi.Application.Abstractions.Realtime;
using MessagingApi.Domain.Conversations;
using MessagingApi.Infrastructure;
using MessagingApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ProductApi.Messages.Products;
using SharedLibrary.Domain.Abstractions;
using Testcontainers.PostgreSql;

namespace MessagingApi.Application.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : IAsyncLifetime
{
    private static readonly Guid SellerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18.1")
        .WithDatabase("MessagingApiIntegrationTests")
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
        services.AddDbContext<MessagingDbContext>(options =>
            options.UseNpgsql($"{_database.GetConnectionString()};Pooling=False"));
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<MessagingDbContext>());
        services.AddSingleton(CreateProductClient());
        services.AddSingleton(Substitute.For<IConversationsRealtimeNotifier>());

        _services = services.BuildServiceProvider();

        using var scope = CreateScope();
        await scope.ServiceProvider
            .GetRequiredService<MessagingDbContext>()
            .Database
            .EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();
        await _database.DisposeAsync();
    }

    private static IRequestClient<GetProductDetailsRequest> CreateProductClient()
    {
        var productClient = Substitute.For<IRequestClient<GetProductDetailsRequest>>();
        productClient
            .GetResponse<GetProductDetailsResponse>(
                Arg.Any<GetProductDetailsRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var productId = callInfo.Arg<GetProductDetailsRequest>()!.ProductId;
                var response = new GetProductDetailsResponse(
                    productId,
                    "Product",
                    string.Empty,
                    10,
                    "USD",
                    1,
                    SellerId,
                    null,
                    0,
                    0,
                    true);

                return Task.FromResult<Response<GetProductDetailsResponse>>(
                    new TestResponse<GetProductDetailsResponse>(response));
            });

        return productClient;
    }
}
