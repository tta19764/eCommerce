using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Infrastructure.Repositories;

public abstract class Repository<T, TContext>(TContext dbContext)
    where T : Entity
    where TContext : DbContext
{
    protected readonly DbSet<T> Context = dbContext.Set<T>();

    public async Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Context
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }
    
    public async Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Context
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<T>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
    
    public virtual async Task<IEnumerable<T>> GetPageAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await Context
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    
    public virtual void Add(T entity)
    {
        Context.Add(entity);
    }
    
    public virtual void Update(T entity)
    {
        Context.Update(entity);
    }
    
    public virtual void Delete(T entity)
    {
        Context.Remove(entity);
    }
}