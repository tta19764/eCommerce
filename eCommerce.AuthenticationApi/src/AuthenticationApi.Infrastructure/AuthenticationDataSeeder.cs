using AuthenticationApi.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationApi.Infrastructure;

/// <summary>
/// Seeds default roles and permissions for local development.
/// </summary>
public static class AuthenticationDataSeeder
{
    private static readonly (Guid Id, string Code, string Description)[] Permissions =
    [
        (Guid.Parse("11111111-1111-1111-1111-111111111111"), "products.read", "Read products"),
        (Guid.Parse("11111111-1111-1111-1111-111111111112"), "products.create", "Create products"),
        (Guid.Parse("11111111-1111-1111-1111-111111111113"), "products.update", "Update products"),
        (Guid.Parse("11111111-1111-1111-1111-111111111114"), "products.delete", "Delete products"),
        (Guid.Parse("11111111-1111-1111-1111-111111111115"), "orders.read-own", "Read own orders"),
        (Guid.Parse("11111111-1111-1111-1111-111111111116"), "orders.create", "Create orders"),
        (Guid.Parse("11111111-1111-1111-1111-111111111117"), "orders.read", "Read all orders"),
        (Guid.Parse("11111111-1111-1111-1111-111111111118"), "orders.update-status", "Update order status"),
        (Guid.Parse("11111111-1111-1111-1111-111111111119"), "users.read", "Read users"),
        (Guid.Parse("11111111-1111-1111-1111-111111111120"), "users.update", "Update users")
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

        await dbContext.Database.MigrateAsync();

        if (!await dbContext.Permissions.AnyAsync())
        {
            dbContext.Permissions.AddRange(Permissions.Select(permission =>
                Permission.Create(permission.Id, new PermissionCode(permission.Code), permission.Description)));
        }

        if (!await dbContext.Roles.AnyAsync(role => role.Name.Value == "Customer"))
        {
            var customer = Role.Create(Guid.Parse("22222222-2222-2222-2222-222222222221"), new RoleName("Customer"));
            Attach(customer, "products.read", "orders.read-own", "orders.create");
            dbContext.Roles.Add(customer);
        }

        if (!await dbContext.Roles.AnyAsync(role => role.Name.Value == "Admin"))
        {
            var admin = Role.Create(Guid.Parse("22222222-2222-2222-2222-222222222222"), new RoleName("Admin"));
            Attach(admin, Permissions.Select(permission => permission.Code).ToArray());
            dbContext.Roles.Add(admin);
        }

        await dbContext.SaveChangesAsync();

        void Attach(Role role, params string[] permissionCodes)
        {
            foreach (var permissionCode in permissionCodes)
            {
                var permission = dbContext.Permissions.Local.FirstOrDefault(permission => permission.Code.Value == permissionCode) ??
                                 dbContext.Permissions.First(permission => permission.Code.Value == permissionCode);

                role.AttachPermission(permission);
            }
        }
    }
}

