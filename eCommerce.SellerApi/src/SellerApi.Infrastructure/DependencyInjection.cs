using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SellerApi.Infrastructure.Repositories;
using SellerApi.Infrastructure.Bootstrap;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;

namespace SellerApi.Infrastructure;

/// <summary>
/// Registers SellerApi infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds persistence, messaging, shared infrastructure, and development bootstrap services.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The configuration that supplies database, broker, and bootstrap settings.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure<SellerDbContext>(configuration);

        AddPersistence(services);
        AddBootstrap(services, configuration);
        services.AddSharedMessaging(configuration, typeof(SellerApi.Application.DependencyInjection).Assembly);

        return services;
    }

    /// <summary>Registers repositories and the SellerDbContext unit of work.</summary>
    /// <param name="services">The service collection to update.</param>
    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<ISellerRepository, SellerRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IStoreReviewRepository, StoreReviewRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<SellerDbContext>());
    }

    /// <summary>Registers marketplace-store options and the development bootstrap worker.</summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The configuration that supplies marketplace-store settings.</param>
    private static void AddBootstrap(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MarketplaceStoreOptions>()
            .Bind(configuration.GetSection(MarketplaceStoreOptions.SectionName));

        services.AddHostedService<MarketplaceStoreHostedService>();
    }
}
