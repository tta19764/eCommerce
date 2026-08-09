using System.Linq.Expressions;

namespace ImageApi.Domain.Images;

/// <summary>
/// Defines the IImageRepository interface used by this slice.
/// </summary>
public interface IImageRepository
{
    /// <summary>
    /// Gets a tracked image aggregate by identifier for reading or mutation through domain methods.
    /// </summary>
    Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Image?> GetByAsync(Expression<Func<Image, bool>> predicate, CancellationToken cancellationToken = default);

    void Add(Image image);

    void Delete(Image image);
}
