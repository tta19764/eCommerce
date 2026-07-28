using System.Linq.Expressions;

namespace ImageApi.Domain.Images;

public interface IImageRepository
{
    Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Image?> GetByAsync(Expression<Func<Image, bool>> predicate, CancellationToken cancellationToken = default);

    void Add(Image image);

    void Delete(Image image);
}
