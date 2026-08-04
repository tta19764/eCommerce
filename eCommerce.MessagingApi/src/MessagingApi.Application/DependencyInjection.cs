using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Application;

namespace MessagingApi.Application;

/// <summary>
/// Registers Messaging API application-layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds MediatR, validation, and shared application behaviors.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSharedApplication(typeof(DependencyInjection).Assembly);
        return services;
    }
}
