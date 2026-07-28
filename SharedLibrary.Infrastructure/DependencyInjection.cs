using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Infrastructure.Options;

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
        AddPersistence<TContext>(services, configuration);
        AddGatewayOptions(services, configuration);
        
        return services;
    }
    
    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        
        services.Configure<AuthenticationOptions>(configuration.GetSection("Authentication"));
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
    
    private static void AddGatewayOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GatewayOptions>(configuration.GetSection("Gateway"));
        services.ConfigureOptions<GatewayOptionsSetup>();
    }
}
