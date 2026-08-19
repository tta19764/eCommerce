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
    /// Registers OrderApi handlers, validators, pricing, and domain-event dispatch services.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same service collection for chained registration.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSharedApplication(typeof(DependencyInjection).Assembly);
        services.AddScoped<OrderStatusChangedNotificationDispatcher>();
        services.AddScoped<SellerOrderStatusChangedIntegrationEventPublisher>();
        services.AddScoped<IOrderPricingService, OrderPricingService>();

        return services;
    }
}
