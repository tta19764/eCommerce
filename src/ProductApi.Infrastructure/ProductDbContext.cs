using Microsoft.EntityFrameworkCore;
using ProductApi.Domain.Products;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Infrastructure;

/// <summary>
/// EF Core database context and unit of work for product persistence.
/// </summary>
public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Product catalog table.
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Applies all entity configurations from the infrastructure assembly.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
