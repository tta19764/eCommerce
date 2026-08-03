using ImageApi.Domain.Images;
using SharedLibrary.Infrastructure.Repositories;

namespace ImageApi.Infrastructure.Repositories;

/// <summary>
/// Defines the ImageRepository class used by this slice.
/// </summary>
public sealed class ImageRepository(ImageDbContext dbContext)
    : Repository<Image, ImageDbContext>(dbContext), IImageRepository;
