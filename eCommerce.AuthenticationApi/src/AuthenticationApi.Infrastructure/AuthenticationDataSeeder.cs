using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationApi.Infrastructure;

/// <summary>
/// Seeds default roles and permissions for local development.
/// </summary>
public static class AuthenticationDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
