using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Application;

namespace OrderApi.Application;

/// <summary>
/// Registers Order API application-layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSharedApplication();
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
