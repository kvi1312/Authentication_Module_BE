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
    public IRoleRepository RolesRepository { get; }
    public IRefreshTokenRepository RefreshTokensRepository { get; }
    public IUserSessionRepository UserSessionsRepository { get; }
    public IRememberMeTokenRepository RememberMeTokensRepository { get; }

    public UnitOfWork(IUserRepository userRepository, DbFactory dbFactory, IRoleRepository rolesRepository,
        IRefreshTokenRepository refreshTokensRepository, IUserSessionRepository userSessionsRepository,
        IRememberMeTokenRepository rememberMeTokensRepository)
    {
        UserRepository = userRepository;
        _dbFactory = dbFactory;
        RolesRepository = rolesRepository;
        RefreshTokensRepository = refreshTokensRepository;
        UserSessionsRepository = userSessionsRepository;
        RememberMeTokensRepository = rememberMeTokensRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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
