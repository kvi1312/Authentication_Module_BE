using Authentication.Domain.Entities;

namespace Authentication.Domain.Interfaces.Repositories;

public interface IUserRepository : IRepositoryBase<User>
{
    Task<User> GetUserByName(string userName);
}
