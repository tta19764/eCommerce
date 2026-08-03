using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationApi.Infrastructure;

/// <summary>
/// Seeds default roles and permissions for local development.
/// </summary>
public static class AuthenticationDataSeeder
{
    /// <summary>
    /// Executes the SeedAsync operation.
    /// </summary>
    /// <param name="serviceProvider">The serviceProvider value.</param>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

        // Static roles and permissions are seeded by EF model data in the migration.
        await dbContext.Database.MigrateAsync();
    }
}
