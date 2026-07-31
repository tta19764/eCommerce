using SharedLibrary.Api.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SharedLibrary.Api.Extensions;

/// <summary>
/// Provides shared ASP.NET Core application builder extensions.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures Serilog using the application configuration.
    /// </summary>
    /// <param name="host">The host builder to configure.</param>
    /// <returns>The same host builder so calls can be chained.</returns>
    public static IHostBuilder UseSharedSerilog(this IHostBuilder host)
    {
        return host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));
    }
    
    /// <summary>
    /// Applies pending Entity Framework Core migrations for the supplied database context.
    /// </summary>
    /// <param name="app">The application builder that provides the service provider.</param>
    /// <typeparam name="TContext">The database context type whose migrations should be applied.</typeparam>
    public static void ApplyMigrations<TContext>(this IApplicationBuilder app) where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        dbContext.Database.Migrate();
    }
    
    /// <summary>
    /// Adds shared exception handling, gateway validation, and request logging middleware.
    /// </summary>
    /// <param name="app">The application builder to add middleware to.</param>
    /// <returns>The same application builder so calls can be chained.</returns>
    public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
    {
        return app
            .UseMiddleware<ExceptionHandlingMiddleware>()
            .UseMiddleware<GatewayOnlyMiddleware>()
            .UseMiddleware<RequestContextLoggingMiddleware>()
            .UseAuthentication()
            .UseAuthorization();
    }
}
