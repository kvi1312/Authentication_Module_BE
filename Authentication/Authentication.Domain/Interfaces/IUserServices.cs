using Authentication.Domain.Dtos.Request;
using Authentication.Domain.Entities;

namespace Authentication.Domain.Interfaces;

public interface IUserServices
{
    // only for admin and manger
    Task<User> GetUserByName(string userName);
    User GetUserById(string email);

    // only user can register
    bool RegisterUser(RegisterUserDto dto);

    // ONLY admin
    User UpgradeRoleForUser(string userId, string roleId);

    // Admin and manager
    bool DeactiveUser(string userId);
}
