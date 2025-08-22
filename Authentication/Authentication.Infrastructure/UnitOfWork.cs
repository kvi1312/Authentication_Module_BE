using Authentication.Domain.Interfaces;
using Authentication.Domain.Interfaces.Repositories;

namespace Authentication.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbFactory _dbFactory;

    public Task RollbackTransactionAsync()
    {
        throw new NotImplementedException();
    }

    public IUserRepository UserRepository { get; set; }
    public IRoleRepository Roles { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IUserSessionRepository UserSessions { get; }
    public IRememberMeTokenRepository RememberMeTokens { get; }

    public UnitOfWork(IUserRepository userRepository, DbFactory dbFactory)
    {
        UserRepository = userRepository;
        _dbFactory = dbFactory;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _dbFactory.DbContext.SaveChangesAsync(cancellationToken);
    }

    public Task BeginTransactionAsync()
    {
        throw new NotImplementedException();
    }

    public Task CommitTransactionAsync()
    {
        throw new NotImplementedException();
    }
}
