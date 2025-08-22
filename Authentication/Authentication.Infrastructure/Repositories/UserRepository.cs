using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(DbFactory dbFactory) : base(dbFactory)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username) =>
        await DbSet.FirstOrDefaultAsync(x => x.Username == username);

    public async Task<User?> GetByEmailAsync(string email) => await DbSet.FirstOrDefaultAsync(x => x.Email == email);

    public async Task<User?> GetWithRolesAsync(Guid id)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetWithRolesAndTokensAsync(Guid id)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public Task<bool> ExistsAsync(string username, string email) => DbSet.AnyAsync(x => x.Username == username && x.Email == email);

    public async Task<IEnumerable<User>> GetByUserTypeAsync(UserType userType)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role.UserType == userType))
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await DbSet
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public async Task<User?> GetByUsernameWithUserTypeAsync(string username, UserType userType)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username &&
                                u.UserRoles.Any(ur => ur.Role.UserType == userType));
    }
}