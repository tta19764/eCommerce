using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using SellerApi.Domain.Sellers;
using SellerApi.Infrastructure;
using SellerApi.Infrastructure.Bootstrap;
using SellerApi.Infrastructure.Repositories;
using SharedLibrary.Domain.Abstractions;
using Testcontainers.PostgreSql;
using UserApi.Messages.Users;

namespace SellerApi.Application.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18.1")
        .WithDatabase("SellerApiIntegrationTests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    private readonly Dictionary<Guid, GetUserDetailsResponse> _users = [];

    private ServiceProvider _services = null!;

    public IServiceScope CreateScope() => _services.CreateScope();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        var userClient = Substitute.For<IRequestClient<GetUserDetailsRequest>>();
        userClient
            .GetResponse<GetUserDetailsResponse>(
                Arg.Any<GetUserDetailsRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<GetUserDetailsRequest>()!;
                var response = _users.GetValueOrDefault(
                    request.UserId,
                    new GetUserDetailsResponse(request.UserId, string.Empty, string.Empty, false));

                return Task.FromResult<Response<GetUserDetailsResponse>>(
                    new TestResponse<GetUserDetailsResponse>(response));
            });

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddDbContext<SellerDbContext>(options =>
            options.UseNpgsql($"{_database.GetConnectionString()};Pooling=False"));
        services.AddScoped<ISellerRepository, SellerRepository>();
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<SellerDbContext>());
        services.AddSingleton<IOptions<MarketplaceStoreOptions>>(
            Options.Create(new MarketplaceStoreOptions()));
        services.AddSingleton(userClient);

        _services = services.BuildServiceProvider();

        using var scope = CreateScope();
        await scope.ServiceProvider
            .GetRequiredService<SellerDbContext>()
            .Database
            .EnsureCreatedAsync();
    }

    public void AddUser(Guid userId, string fullName, string email)
    {
        _users[userId] = new GetUserDetailsResponse(userId, fullName, email, true);
    }

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();
        await _database.DisposeAsync();
    }
}
