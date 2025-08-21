using Authentication.Domain.Interfaces;
using Authentication.Domain.Interfaces.Repositories;

namespace Authentication.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbFactory _dbFactory;

    public IUserRepository UserRepository { get; set; }

    public UnitOfWork(IUserRepository userRepository, DbFactory dbFactory)
    {
        UserRepository = userRepository;
        _dbFactory = dbFactory;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _dbFactory.DbContext.SaveChangesAsync(cancellationToken);
    }
}
