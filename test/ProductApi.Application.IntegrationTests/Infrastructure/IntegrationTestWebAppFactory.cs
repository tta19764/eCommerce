using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Domain.Products;
using ProductApi.Infrastructure;
using ProductApi.Infrastructure.Repositories;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public IntegrationTestWebAppFactory()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddApplication();
        services.AddDbContext<ProductDbContext>(options =>
            options.UseInMemoryDatabase($"product-api-tests-{Guid.NewGuid():N}"));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ProductDbContext>());

        _serviceProvider = services.BuildServiceProvider();
    }

    public IServiceScope CreateScope()
    {
        return _serviceProvider.CreateScope();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
}
