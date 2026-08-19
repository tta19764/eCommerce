using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Application;

namespace SellerApi.Application;

/// <summary>
/// Registers SellerApi application-layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers SellerApi handlers, validators, and pipeline behaviors.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSharedApplication(typeof(DependencyInjection).Assembly);

        return services;
    }
}
