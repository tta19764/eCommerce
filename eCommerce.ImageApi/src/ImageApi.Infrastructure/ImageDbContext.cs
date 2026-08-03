using ImageApi.Domain.Images;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Infrastructure;

/// <summary>
/// Defines the ImageDbContext class used by this slice.
/// </summary>
public sealed class ImageDbContext(DbContextOptions<ImageDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Image> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ImageDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
