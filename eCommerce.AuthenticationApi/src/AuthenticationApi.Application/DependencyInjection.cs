using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Application;

namespace AuthenticationApi.Application;

/// <summary>
/// Registers Authentication API application-layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers AuthenticationApi request handlers and application pipeline behaviors.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same service collection for chained registration.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services.AddSharedApplication(typeof(DependencyInjection).Assembly);
    }
}
