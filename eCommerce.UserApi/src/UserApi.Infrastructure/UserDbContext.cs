using Microsoft.EntityFrameworkCore;
using SharedLibrary.Domain.Abstractions;
using UserApi.Domain.Users;

namespace UserApi.Infrastructure;

/// <summary>
/// EF Core database context and unit of work for user profile persistence.
/// </summary>
public sealed class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Users table.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Applies all entity configurations from the infrastructure assembly.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
