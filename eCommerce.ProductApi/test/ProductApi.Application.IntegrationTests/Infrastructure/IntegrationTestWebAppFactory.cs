using ImageApi.Messages.Images;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OrderApi.Messages.Orders;
using ProductApi.Domain.Categories;
using ProductApi.Application.IntegrationTests.Products;
using ProductApi.Domain.Products;
using ProductApi.Domain.Reviews;
using SharedLibrary.Application.Abstractions.Caching;
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
        var imageClient = Substitute.For<IRequestClient<AddProductImagesRequest>>();
        imageClient
            .GetResponse<AddProductImagesResponse>(
                Arg.Any<AddProductImagesRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<AddProductImagesRequest>()!;
                var imageIds = request.TemporaryImageIds.ToArray();

                return Task.FromResult<Response<AddProductImagesResponse>>(
                    new TestResponse<AddProductImagesResponse>(
                        new AddProductImagesResponse(true, imageIds, [])));
            });

        var cacheService = Substitute.For<ICacheService>();
        var purchaseStatusClient = Substitute.For<IRequestClient<GetUserProductPurchaseStatusRequest>>();
        purchaseStatusClient
            .GetResponse<GetUserProductPurchaseStatusResponse>(
                Arg.Any<GetUserProductPurchaseStatusRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var request = callInfo.Arg<GetUserProductPurchaseStatusRequest>()!;
                return Task.FromResult<Response<GetUserProductPurchaseStatusResponse>>(
                    new TestResponse<GetUserProductPurchaseStatusResponse>(
                        new GetUserProductPurchaseStatusResponse(
                            request.UserId,
                            request.ProductId,
                            true,
                            true)));
            });

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddApplication();
        services.AddDbContext<ProductDbContext>(options =>
            options.UseNpgsql($"{_dbContainer.GetConnectionString()};Pooling=False"));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ProductDbContext>());
        services.AddSingleton(imageClient);
        services.AddSingleton(purchaseStatusClient);
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
        await scope.ServiceProvider.GetRequiredService<ProductDbContext>().Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
