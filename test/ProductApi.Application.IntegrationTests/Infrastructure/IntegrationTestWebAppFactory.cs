using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Domain.Products;
using ProductApi.Infrastructure;
using ProductApi.Infrastructure.Repositories;
using SharedLibrary.Domain.Abstractions;
using Testcontainers.PostgreSql;
using Xunit;

namespace ProductApi.Application.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18.1")
        .WithDatabase("eCommerceProductApiTest")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly ServiceProvider _serviceProvider;

    public IntegrationTestWebAppFactory()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddApplication();
        services.AddDbContext<ProductDbContext>(options =>
            options.UseNpgsql($"{_dbContainer.GetConnectionString()};Pooling=False"));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ProductDbContext>());

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
        await scope.ServiceProvider.GetRequiredService<ProductDbContext>().Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
