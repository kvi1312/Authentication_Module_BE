using Authentication.Domain.Interfaces.Repositories;

namespace Authentication.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();

    IUserRepository UserRepository { get; }
    IRoleRepository Roles { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IUserSessionRepository UserSessions { get; }
    IRememberMeTokenRepository RememberMeTokens { get; }
}
