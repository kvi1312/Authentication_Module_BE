using Authentication.Application.Interfaces;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Authentication.Application.Strategies;

public class PartnerAuthenticationStrategy : IAuthenticationStrategy
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<PartnerAuthenticationStrategy> _logger;

    public UserType UserType => UserType.Partner;

    public PartnerAuthenticationStrategy(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ILogger<PartnerAuthenticationStrategy> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<User?> ValidateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByUsernameWithUserTypeAsync(username, UserType.Partner);
        
        if (user == null)
        {
            _logger.LogWarning("Partner user not found: {Username}", username);
            return null;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Partner account is inactive: {Username}", username);
            return null;
        }

        if (!_passwordService.VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for partner user: {Username}", username);
            return null;
        }

        // Additional partner specific validation
        var userWithRoles = await _unitOfWork.UserRepository.GetWithRolesAsync(user.Id);
        if (!userWithRoles.HasUserType(UserType.Partner))
        {
            _logger.LogWarning("User does not have Partner role: {Username}", username);
            return null;
        }

        return userWithRoles;
    }

    public async Task<Dictionary<string, object>> GetAdditionalClaimsAsync(User user, CancellationToken cancellationToken = default)
    {
        var claims = new Dictionary<string, object>
        {
            { "user_type", UserType.Partner.ToString() },
            { "is_partner_admin", user.HasRole("PartnerAdmin") },
            { "partner_level", GetPartnerLevel(user) }
        };

        return claims;
    }

    private string GetPartnerLevel(User user)
    {
        if (user.HasRole("PartnerAdmin")) return "admin";
        if (user.HasRole("Partner")) return "standard";
        return "user";
    }
}