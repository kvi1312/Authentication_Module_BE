using Authentication.Domain.Dtos.Request;
using Authentication.Domain.Entities;
using Authentication.Domain.Interfaces.Repositories;

namespace Authentication.Domain.Interfaces;

public class UserServices : IUserServices
{
    private readonly IUserRepository _userRepository;

    public UserServices(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public bool DeactiveUser(string userId)
    {
        throw new NotImplementedException();
    }

    public User GetUserById(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<User> GetUserByName(string userName) => await _userRepository.GetUserByName(userName);

    public bool RegisterUser(RegisterUserDto dto)
    {
        throw new NotImplementedException();
    }

    public User UpgradeRoleForUser(string userId, string roleId)
    {
        throw new NotImplementedException();
    }
}
