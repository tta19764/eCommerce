using eCommerce.SharedLibrary.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.SharedLibrary;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices<TContext>(this IServiceCollection services,
        IConfiguration configuration, string fileName) where TContext : DbContext
    {
        var connectionString =
            configuration.GetConnectionString("Database") ??
            throw new ArgumentNullException(nameof(configuration));

        services.AddDbContext<TContext>(options =>
        {
            options.UseSqlServer(connectionString, 
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());
        });
        
        AddGatewayOptions(services, configuration);
        AddJwtAuthenticationScheme(services, configuration);
        
        return services;
    }
    
    private static void AddJwtAuthenticationScheme(IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        
        services.Configure<AuthenticationOptions>(configuration.GetSection("Authentication"));
        services.ConfigureOptions<JwtBearerOptionsSetup>();
    }
    
    private static void AddGatewayOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GatewayOptions>(configuration.GetSection("Gateway"));
        services.ConfigureOptions<GatewayOptionsSetup>();
    }
}