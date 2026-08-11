using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Application;

namespace PaymentApi.Application;

/// <summary>Registers PaymentApi command/query handlers, validators, and shared application behaviors.</summary>
public static class DependencyInjection
{
    /// <summary>Adds the PaymentApi application assembly to the shared mediator pipeline.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSharedApplication(typeof(DependencyInjection).Assembly);
        return services;
    }
}
