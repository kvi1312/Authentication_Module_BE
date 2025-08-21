using Authentication.Domain.Interfaces.Repositories;

namespace Authentication.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    public IUserRepository UserRepository { get; }
}
