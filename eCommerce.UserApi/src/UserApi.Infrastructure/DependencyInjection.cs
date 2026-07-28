using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;
using UserApi.Domain.Users;
using UserApi.Infrastructure.Repositories;

namespace UserApi.Infrastructure;

/// <summary>
/// Registers User API infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds persistence, repository, authentication, gateway options, and messaging services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure<UserDbContext>(configuration);

        AddPersistence(services);
        services.AddSharedMessaging(configuration, typeof(UserApi.Application.DependencyInjection).Assembly);

        return services;
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<UserDbContext>());
    }
}
