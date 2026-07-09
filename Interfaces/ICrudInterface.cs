using System.Linq.Expressions;
using eCommerce.SharedLibrary.Responses;

namespace eCommerce.SharedLibrary.Interfaces;

public interface ICrudInterface<T> where T : class
{
    Task<Response> CreateAsync(T entity);
    Task<Response> UpdateAsync(T entity);
    Task<Response> DeleteAsync(T entity);
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetByAsync(Expression<Func<T, bool>> predicate);
}