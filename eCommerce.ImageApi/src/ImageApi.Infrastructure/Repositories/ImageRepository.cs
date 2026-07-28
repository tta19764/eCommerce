using ImageApi.Domain.Images;
using SharedLibrary.Infrastructure.Repositories;

namespace ImageApi.Infrastructure.Repositories;

public sealed class ImageRepository(ImageDbContext dbContext)
    : Repository<Image, ImageDbContext>(dbContext), IImageRepository;
