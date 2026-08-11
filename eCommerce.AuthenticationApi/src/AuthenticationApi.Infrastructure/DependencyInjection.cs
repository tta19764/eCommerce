using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Infrastructure.Authentication;
using AuthenticationApi.Infrastructure.Bootstrap;
using AuthenticationApi.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;

namespace AuthenticationApi.Infrastructure;

/// <summary>
/// Registers Authentication API infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Executes the AddInfrastructure operation.
    /// </summary>
    /// <param name="services">The services value.</param>
    /// <param name="configuration">The configuration value.</param>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure<AuthenticationDbContext>(configuration);

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<AuthenticationDbContext>());

        AddKeycloakIdentityProvider(services, configuration);

        services.Configure<AdminBootstrapOptions>(
            configuration.GetSection(AdminBootstrapOptions.SectionName));
        services.AddHostedService<AdminBootstrapHostedService>();

        services.AddSharedMessaging(configuration, typeof(AuthenticationApi.Application.DependencyInjection).Assembly);

        return services;
    }

    private static void AddKeycloakIdentityProvider(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));

        services.AddTransient<AdminAuthorizationDelegatingHandler>();

        services
            .AddHttpClient<IIdentityProvider, KeycloakIdentityProvider>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
                httpClient.BaseAddress = new Uri(options.AdminUrl);
            })
            .AddHttpMessageHandler<AdminAuthorizationDelegatingHandler>();

        services.AddHttpClient("Keycloak.Token", (serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
            httpClient.BaseAddress = new Uri(options.TokenUrl);
        });
    }
}
