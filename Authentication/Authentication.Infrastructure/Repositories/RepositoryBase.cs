using Authentication.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Authentication.Infrastructure.Repositories;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    private readonly DbFactory _dbFactory;
    private DbSet<T> _dbSet;
    protected DbSet<T> DbSet => _dbSet ?? _dbFactory.DbContext.Set<T>();


    public RepositoryBase(DbFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<T> FindAsync(string id) =>  await DbSet.FindAsync(id);
    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public void AddAsync(T entity) => DbSet.AddAsync(entity);

    public void DeleteAsync(T entity) => DbSet.Remove(entity);

    public void UpdateAsync(T entity) => DbSet.Update(entity);

    public void Find(string id) => DbSet.Find(id);
}
