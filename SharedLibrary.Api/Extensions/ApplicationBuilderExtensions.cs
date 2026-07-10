using SharedLibrary.Api.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SharedLibrary.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IHostBuilder UseSharedSerilog(this IHostBuilder host)
    {
        return host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));
    }
    
    public static void ApplyMigrations<TContext>(this IApplicationBuilder app) where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        dbContext.Database.Migrate();
    }
    
    public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
    {
        return app
            .UseMiddleware<ExceptionHandlingMiddleware>()
            .UseMiddleware<GatewayOnlyMiddleware>()
            .UseMiddleware<RequestContextLoggingMiddleware>();
    }
}