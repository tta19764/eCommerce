using Microsoft.EntityFrameworkCore;
using OrderApi.Domain.Orders;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Infrastructure;

/// <summary>
/// EF Core database context and unit of work for order persistence.
/// </summary>
public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Orders table.
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// Order items table.
    /// </summary>
    public DbSet<OrderItem> OrderItems { get; set; }

    /// <summary>
    /// Applies all entity configurations from the infrastructure assembly.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
