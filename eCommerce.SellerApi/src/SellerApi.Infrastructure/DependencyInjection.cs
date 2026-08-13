using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SellerApi.Domain.Sellers;
using SellerApi.Infrastructure.Repositories;
using SellerApi.Infrastructure.Bootstrap;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;

namespace SellerApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure<SellerDbContext>(configuration);
        services.AddScoped<ISellerRepository, SellerRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SellerDbContext>());
        services.AddOptions<MarketplaceStoreOptions>()
            .Bind(configuration.GetSection(MarketplaceStoreOptions.SectionName));
        services.AddHostedService<MarketplaceStoreHostedService>();
        services.AddSharedMessaging(configuration, typeof(SellerApi.Application.DependencyInjection).Assembly);
        return services;
    }
}
