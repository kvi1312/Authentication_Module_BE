using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.Repositories;

public class RoleRepository : RepositoryBase<Role>, IRoleRepository
{
    public RoleRepository(DbFactory dbFactory) : base(dbFactory)
    {
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await DbSet.FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<IEnumerable<Role>> GetByUserTypeAsync(UserType userType)
    {
        return await DbSet.Where(r => r.UserType == userType).ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await DbSet.AnyAsync(r => r.Name == name);
    }
}
