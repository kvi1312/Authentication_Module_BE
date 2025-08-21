using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure;

public class DbFactory : IDisposable
{
    private bool _disposed;
    private readonly AppDbContext _dbContext;
    public DbContext DbContext => _dbContext;

    public DbFactory(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _dbContext.Dispose();
        }
    }
}
