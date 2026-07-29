using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Domain.Products;
using ProductApi.Domain.Reviews;
using ProductApi.Infrastructure.Repositories;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;

namespace ProductApi.Infrastructure;

/// <summary>
/// Registers Product API infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds persistence, repository, authentication, and gateway infrastructure services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure<ProductDbContext>(configuration);

        AddPersistence(services);
        services.AddSharedMessaging(configuration, typeof(ProductApi.Application.DependencyInjection).Assembly);

        return services;
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ProductDbContext>());
    }
}
