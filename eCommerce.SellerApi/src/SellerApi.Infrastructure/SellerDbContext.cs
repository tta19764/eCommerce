using Microsoft.EntityFrameworkCore;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Infrastructure;

/// <summary>
/// EF Core database context and unit of work for seller, store, and review persistence.
/// </summary>
/// <param name="options">The EF Core options for the seller database.</param>
public sealed class SellerDbContext(DbContextOptions<SellerDbContext> options) : DbContext(options), IUnitOfWork
{
    /// <summary>Gets the seller application set.</summary>
    public DbSet<Seller> Sellers => Set<Seller>();

    /// <summary>Gets the public store set.</summary>
    public DbSet<Store> Stores => Set<Store>();

    /// <summary>Gets the store review set.</summary>
    public DbSet<StoreReview> StoreReviews => Set<StoreReview>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SellerDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
