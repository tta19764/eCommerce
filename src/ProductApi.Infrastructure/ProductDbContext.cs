using Microsoft.EntityFrameworkCore;
using ProductApi.Domain.Categories;
using ProductApi.Domain.Products;
using ProductApi.Domain.Reviews;
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
    /// Marketplace category tree table.
    /// </summary>
    public DbSet<ProductCategory> ProductCategories { get; set; }

    /// <summary>
    /// Product reviews table.
    /// </summary>
    public DbSet<ProductReview> ProductReviews { get; set; }

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
