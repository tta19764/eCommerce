using AuthenticationApi.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Infrastructure;

/// <summary>
/// EF Core database context and unit of work for authentication data.
/// </summary>
public sealed class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Account> Accounts { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<Permission> Permissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthenticationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}

