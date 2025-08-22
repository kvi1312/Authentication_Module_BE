using Authentication.Domain.Entities;
using Authentication.Domain.Enums;

namespace Authentication.Domain.Services;

public interface IDomainUserService
{
    Task<bool> CanUserAccessUserTypeAsync(User user, UserType requestedUserType);
    Task<bool> IsUsernameAvailableAsync(string username, Guid? excludeUserId = null);
    Task<bool> IsEmailAvailableAsync(string email, Guid? excludeUserId = null);
    Task<User> CreateUserWithDefaultRoleAsync(string username, string email, string passwordHash,
        string firstName, string lastName, UserType userType);
}