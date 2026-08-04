using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Application;

namespace ProductApi.Application;

/// <summary>
/// Registers Product API application-layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds MediatR handlers, validators, and shared application pipeline behavior.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services.AddSharedApplication(typeof(DependencyInjection).Assembly);
    }
}
