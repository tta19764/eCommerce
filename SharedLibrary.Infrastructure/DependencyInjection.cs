using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using SharedLibrary.Application.Authorization;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Infrastructure.Authorization;
using SharedLibrary.Infrastructure.Options;
using SharedLibrary.Infrastructure.Caching;
using System.Reflection;
using MassTransit;

namespace SharedLibrary.Infrastructure;

/// <summary>
/// Registers shared infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds shared infrastructure services, including authentication, persistence, and gateway options.
    /// </summary>
    /// <param name="services">The service collection to register infrastructure services into.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <typeparam name="TContext">The database context type used for persistence.</typeparam>
    /// <returns>The same service collection so calls can be chained.</returns>
    public static IServiceCollection AddSharedInfrastructure<TContext>(this IServiceCollection services, IConfiguration configuration) where TContext: DbContext
    {
        AddJwtAuthentication(services, configuration);
        AddCaching(services, configuration);
        AddPersistence<TContext>(services, configuration);
        AddGatewayOptions(services, configuration);
        
        return services;
    }

    /// <summary>
    /// Adds MassTransit with RabbitMQ and registers consumers from the supplied assemblies.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="consumerAssemblies">Assemblies that contain MassTransit consumers for the service.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddSharedMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] consumerAssemblies)
    {
        services.AddMassTransit(busConfigurator =>
        {
            foreach (var consumerAssembly in consumerAssemblies.Distinct())
            {
                busConfigurator.AddConsumers(consumerAssembly);
            }

            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                var connectionString = configuration.GetConnectionString("rabbitmq")
                    ?? configuration.GetConnectionString("RabbitMQ")
                    ?? throw new ArgumentException("'rabbitmq' connection string cannot be null.");

                configurator.Host(new Uri(connectionString));
                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
    
    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddAuthorization(options =>
        {
            foreach (var permission in ApplicationPermissions.All)
            {
                options.AddPolicy(
                    permission,
                    policy => policy
                        .RequireAuthenticatedUser()
                        .AddRequirements(new PermissionRequirement(permission)));
            }
        });

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IClaimsTransformation, KeycloakRoleClaimsTransformation>();
        
        services.Configure<Options.AuthenticationOptions>(configuration.GetSection("Authentication"));
        services.ConfigureOptions<JwtBearerOptionsSetup>();
    }

    private static void AddPersistence<TContext>(IServiceCollection services, IConfiguration configuration) where TContext: DbContext
    {
        var connectionString =
            configuration.GetConnectionString("Database") ??
            throw new ArgumentNullException(nameof(configuration));

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure());
        });
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? configuration.GetConnectionString("redis");

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });
        }

        services.AddSingleton<ICacheService, CacheService>();
    }
    
    private static void AddGatewayOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GatewayOptions>(configuration.GetSection("Gateway"));
        services.ConfigureOptions<GatewayOptionsSetup>();
    }
}
