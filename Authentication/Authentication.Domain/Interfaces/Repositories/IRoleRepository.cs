using Authentication.Domain.Entities;
using Authentication.Domain.Enums;

namespace Authentication.Domain.Interfaces.Repositories;

public interface IRoleRepository : IRepositoryBase<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetByUserTypeAsync(UserType userType);
    Task<bool> ExistsByNameAsync(string name);
    Task<Role?> GetByRoleTypeAsync(RoleType roleType);
    Task<IEnumerable<Role>> GetByRoleTypesAsync(IEnumerable<RoleType> roleTypes);
}
