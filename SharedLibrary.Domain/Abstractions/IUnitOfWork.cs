namespace SharedLibrary.Domain.Abstractions;

/// <summary>
/// Coordinates persistence of changes made within a unit of work.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists pending changes to the underlying store.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The number of state entries written to the underlying store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
