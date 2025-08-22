using Authentication.Domain.Interfaces.Repositories;

namespace Authentication.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();

    IUserRepository UserRepository { get; }
    IRoleRepository RolesRepository { get; }
    IRefreshTokenRepository RefreshTokensRepository { get; }
    IUserSessionRepository UserSessionsRepository { get; }
    IRememberMeTokenRepository RememberMeTokensRepository { get; }
}
