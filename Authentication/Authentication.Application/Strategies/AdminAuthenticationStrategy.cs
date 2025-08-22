using Authentication.Application.Interfaces;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Authentication.Application.Strategies;

public class AdminAuthenticationStrategy : IAuthenticationStrategy
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<AdminAuthenticationStrategy> _logger;
    
    public UserType UserType => UserType.Admin;

    public AdminAuthenticationStrategy(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ILogger<AdminAuthenticationStrategy> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _logger = logger;
    }
    
    public async Task<User?> ValidateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByUsernameWithUserTypeAsync(username, UserType.Admin);
        
        if (user == null)
        {
            _logger.LogWarning("Admin user not found: {Username}", username);
            return null;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Admin account is inactive: {Username}", username);
            return null;
        }

        if (!_passwordService.VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for admin user: {Username}", username);
            return null;
        }

        // Additional admin specific validation
        var userWithRoles = await _unitOfWork.UserRepository.GetWithRolesAsync(user.Id);
        if (!userWithRoles.HasUserType(UserType.Admin))
        {
            _logger.LogWarning("User does not have Admin role: {Username}", username);
            return null;
        }

        return userWithRoles;
    }

    public async Task<Dictionary<string, object>> GetAdditionalClaimsAsync(User user, CancellationToken cancellationToken = default)
    {
        var claims = new Dictionary<string, object>
        {
            { "user_type", UserType.Admin.ToString() },
            { "is_super_admin", user.HasRole("SuperAdmin") },
            { "admin_level", GetAdminLevel(user) }
        };

        return claims;
    }

    private string GetAdminLevel(User user)
    {
        if (user.HasRole("SuperAdmin")) return "super";
        if (user.HasRole("SystemAdmin")) return "system";
        return "standard";
    }
}