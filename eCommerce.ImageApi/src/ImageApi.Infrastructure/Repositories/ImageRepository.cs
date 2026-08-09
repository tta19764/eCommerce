using ImageApi.Domain.Images;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Infrastructure.Repositories;

namespace ImageApi.Infrastructure.Repositories;

/// <summary>
/// Defines the ImageRepository class used by this slice.
/// </summary>
public sealed class ImageRepository(ImageDbContext dbContext)
    : Repository<Image, ImageDbContext>(dbContext), IImageRepository
{
    /// <summary>
    /// Gets a tracked image aggregate so entity mutations are persisted by the unit of work.
    /// </summary>
    public new async Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(image => image.Id == id, cancellationToken);
    }
}
