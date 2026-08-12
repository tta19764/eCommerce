using Microsoft.EntityFrameworkCore;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Infrastructure;

public sealed class SellerDbContext(DbContextOptions<SellerDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Seller> Sellers => Set<Seller>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreReview> StoreReviews => Set<StoreReview>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Seller>(entity => { entity.HasKey(x => x.Id); entity.Property(x => x.Id).ValueGeneratedNever(); entity.HasIndex(x => x.OwnerUserId).IsUnique(); entity.Property(x => x.RejectionReason).HasMaxLength(1000); });
        modelBuilder.Entity<Store>(entity => { entity.HasKey(x => x.Id); entity.Property(x => x.Id).ValueGeneratedNever(); entity.HasIndex(x => x.SellerId).IsUnique(); entity.HasIndex(x => x.Slug).IsUnique(); entity.Property(x => x.Slug).HasMaxLength(80); entity.Property(x => x.Name).HasMaxLength(120); entity.Property(x => x.Description).HasMaxLength(2000); entity.Ignore(x => x.AverageRating); });
        modelBuilder.Entity<StoreReview>(entity => { entity.HasKey(x => x.Id); entity.Property(x => x.Id).ValueGeneratedNever(); entity.HasIndex(x => new { x.StoreId, x.CustomerUserId }).IsUnique(); entity.HasIndex(x => x.SellerOrderId).IsUnique(); entity.Property(x => x.Comment).HasMaxLength(2000); });
    }
}
