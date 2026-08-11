using Microsoft.Extensions.DependencyInjection;
using OrderApi.Application.Orders.Messaging;
using OrderApi.Application.Orders.Notifications;
using OrderApi.Application.Orders.Pricing;
using SharedLibrary.Application;

namespace OrderApi.Application;

/// <summary>
/// Registers Order API application-layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Executes the AddApplication operation.
    /// </summary>
    /// <param name="services">The services value.</param>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSharedApplication(typeof(DependencyInjection).Assembly);
        services.AddScoped<OrderStatusChangedNotificationDispatcher>();
        services.AddScoped<SellerOrderStatusChangedIntegrationEventPublisher>();
        services.AddScoped<IOrderPricingService, OrderPricingService>();

        return services;
    }
}
