using System.Linq.Expressions;

namespace Authentication.Domain.Interfaces.Repositories;

public interface IRepositoryBase<T> where T : class
{
    Task<T> FindAsync(string id);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    void Find (string id);
    void AddAsync(T entity);
    void DeleteAsync(T entity);
    void UpdateAsync(T entity);
}
