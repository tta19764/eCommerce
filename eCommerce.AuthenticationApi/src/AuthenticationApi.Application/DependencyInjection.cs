using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Application;

namespace AuthenticationApi.Application;

/// <summary>
/// Registers Authentication API application-layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Executes the AddApplication operation.
    /// </summary>
    /// <param name="services">The services value.</param>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services.AddSharedApplication(typeof(DependencyInjection).Assembly);
    }
}
