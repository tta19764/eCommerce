using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Application;

namespace NotificationApi.Application;

/// <summary>
/// Registers Notification API application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds shared application behavior and notification processors.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSharedApplication(typeof(DependencyInjection).Assembly);
        services.AddScoped<NotificationJobProcessor>();

        return services;
    }
}
