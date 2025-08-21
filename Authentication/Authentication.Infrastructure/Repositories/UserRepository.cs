using Authentication.Domain.Entities;
using Authentication.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(DbFactory dbFactory) : base(dbFactory)
    {
    }

    public async Task<User> GetUserByName(string userName) => await DbSet.Where(x => string.Equals(x.Username, userName)).FirstOrDefaultAsync();
}
