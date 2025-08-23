using Authentication.Domain.Entities;
using Authentication.Domain.Enums;

namespace Authentication.Domain.Interfaces.Repositories;

public interface IUserRepository : IRepositoryBase<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetWithRolesAsync(Guid id);
    Task<User?> GetWithRolesByUsernameAsync(string username);
    Task<User?> GetWithRolesAndTokensAsync(Guid id);
    Task<bool> ExistsAsync(string username, string email);
    Task<IEnumerable<User>> GetByUserTypeAsync(UserType userType);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<User?> GetByUsernameWithUserTypeAsync(string username, UserType userType);

    Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(int pageNumber, int pageSize, string? searchTerm = null, UserType? userType = null, RoleType? roleFilter = null);
    Task AddUserRoleAsync(Guid userId, Guid roleId);
    Task RemoveUserRoleAsync(Guid userId, Guid roleId);
    Task<bool> UserHasRoleAsync(Guid userId, Guid roleId);
}
