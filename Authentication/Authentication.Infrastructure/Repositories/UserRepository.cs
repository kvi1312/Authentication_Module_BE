using Authentication.Application.Extensions;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    private readonly DbFactory _dbFactory;

    public UserRepository(DbFactory dbFactory) : base(dbFactory)
    {
        _dbFactory = dbFactory;
    }

    protected AppDbContext AppDbContext => (AppDbContext)_dbFactory.DbContext;

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

    public async Task<User?> GetWithRolesByUsernameAsync(string username)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username);
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

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(int pageNumber, int pageSize, string? searchTerm = null, UserType? userType = null, RoleType? roleFilter = null)
    {
        var query = DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => u.Username.Contains(searchTerm) ||
                                   u.Email.Contains(searchTerm) ||
                                   u.FirstName.Contains(searchTerm) ||
                                   u.LastName.Contains(searchTerm));
        }

        if (userType.HasValue)
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.UserType == userType.Value));
        }

        if (roleFilter.HasValue)
        {
            var roleName = roleFilter.Value.ToRoleName();
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task AddUserRoleAsync(Guid userId, Guid roleId)
    {
        var existingUserRole = await AppDbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (existingUserRole == null)
        {
            var userRole = UserRole.Create(userId, roleId);
            await AppDbContext.UserRoles.AddAsync(userRole);
        }
    }

    public async Task RemoveUserRoleAsync(Guid userId, Guid roleId)
    {
        var userRole = await AppDbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole != null)
        {
            AppDbContext.UserRoles.Remove(userRole);
        }
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId)
    {
        return await AppDbContext.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }
}