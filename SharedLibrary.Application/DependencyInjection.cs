using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Application.Abstractions.Behaviors;

namespace SharedLibrary.Application;

/// <summary>
/// Registers application-layer services, handlers, validators, and MediatR pipeline behaviors.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the application layer to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register application services into.</param>
    /// <returns>The same service collection so calls can be chained.</returns>
    public static IServiceCollection AddSharedApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
