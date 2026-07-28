using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Application.Abstractions.Behaviors;
using System.Reflection;

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
    public static IServiceCollection AddSharedApplication(this IServiceCollection services, params Assembly[] applicationAssemblies)
    {
        var assemblies = new[] { typeof(DependencyInjection).Assembly }
            .Concat(applicationAssemblies)
            .Distinct()
            .ToArray();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblies(assemblies);
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(assemblies);

        return services;
    }
}
