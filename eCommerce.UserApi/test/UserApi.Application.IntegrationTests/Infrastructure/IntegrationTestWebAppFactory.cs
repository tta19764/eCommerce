using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Domain.Abstractions;
using Testcontainers.PostgreSql;
using UserApi.Domain.Users;
using UserApi.Infrastructure;
using UserApi.Infrastructure.Repositories;
using Xunit;

namespace UserApi.Application.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18.1")
        .WithDatabase("eCommerceUserApiTest")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly ServiceProvider _serviceProvider;

    public IntegrationTestWebAppFactory()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddApplication();
        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql($"{_dbContainer.GetConnectionString()};Pooling=False"));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<UserDbContext>());

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
        await scope.ServiceProvider.GetRequiredService<UserDbContext>().Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
