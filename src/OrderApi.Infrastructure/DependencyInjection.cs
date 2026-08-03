using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Domain.Orders;
using OrderApi.Infrastructure.Repositories;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;
using SharedLibrary.Infrastructure.Outbox;

namespace OrderApi.Infrastructure;

/// <summary>
/// Registers Order API infrastructure services.
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
        services.AddSharedInfrastructure<OrderDbContext>(configuration);

        AddPersistence(services);
        services.AddSharedMessaging(configuration, typeof(OrderApi.Application.DependencyInjection).Assembly);
        AddBackgroundJobs(services, configuration);

        return services;
    }

    private static void AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOutboxMessageProcessing<OrderDbContext>(configuration);
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<OrderDbContext>());
    }
}
