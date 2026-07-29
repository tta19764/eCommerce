using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Infrastructure.Repositories;

/// <summary>
/// Provides common Entity Framework Core repository operations for aggregate entities.
/// </summary>
/// <typeparam name="T">The entity type managed by the repository.</typeparam>
/// <typeparam name="TContext">The database context type that exposes the entity set.</typeparam>
public abstract class Repository<T, TContext>(TContext dbContext)
    where T : Entity
    where TContext : DbContext
{
    /// <summary>
    /// Entity set used by derived repositories for query and persistence operations.
    /// </summary>
    protected readonly DbSet<T> DbSet = dbContext.Set<T>();

    /// <summary>
    /// Database context used by derived repositories for query and persistence operations.
    /// </summary>
    protected readonly TContext DbContext = dbContext;

    /// <summary>
    /// Gets the first entity matching the supplied predicate.
    /// </summary>
    /// <param name="predicate">The expression used to filter the entity set.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The matching entity when found; otherwise, <c>null</c>.</returns>
    public async Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Repository reads are not tracked by default because these methods are intended for read models/lookups.
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The entity with the requested identifier when found; otherwise, <c>null</c>.</returns>
    public async Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets all entities from the repository.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A collection containing all entities.</returns>
    public async Task<IEnumerable<T>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets one page of entities.
    /// </summary>
    /// <param name="page">The one-based page number to retrieve.</param>
    /// <param name="pageSize">The maximum number of entities returned in the page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A collection containing the requested page of entities.</returns>
    public virtual async Task<IEnumerable<T>> GetPageAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Counts all entities in the repository.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The total number of entities.</returns>
    public virtual Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return DbSet.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Adds an entity to the underlying context.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    public virtual void Add(T entity)
    {
        DbContext.Add(entity);
    }

    /// <summary>
    /// Marks an entity as modified in the underlying context.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public virtual void Update(T entity)
    {
        DbContext.Update(entity);
    }

    /// <summary>
    /// Removes an entity from the underlying context.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public virtual void Delete(T entity)
    {
        DbContext.Remove(entity);
    }
}
