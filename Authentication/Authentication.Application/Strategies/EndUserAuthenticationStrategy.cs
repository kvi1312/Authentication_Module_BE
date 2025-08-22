// using Authentication.Application.Interfaces;
// using Authentication.Domain.Entities;
// using Authentication.Domain.Enums;
// using Authentication.Domain.Interfaces;
// using Microsoft.Extensions.Logging;
//
// namespace Authentication.Application.Strategies;
//
// public class EndUserAuthenticationStrategy : IAuthenticationStrategy
// {
//     private readonly IUnitOfWork _unitOfWork;
//     private readonly IPasswordService _passwordService;
//     private readonly ILogger<EndUserAuthenticationStrategy> _logger;
//     public UserType UserType => UserType.EndUser;
//     
//     public EndUserAuthenticationStrategy(
//         IUnitOfWork unitOfWork,
//         IPasswordService passwordService,
//         ILogger<EndUserAuthenticationStrategy> logger)
//     {
//         _unitOfWork = unitOfWork;
//         _passwordService = passwordService;
//         _logger = logger;
//     }
//     
//     public async Task<User?> ValidateAsync(string username, string password, CancellationToken cancellationToken = default)
//     {
//         var user = _unitOfWork.UserRepository.GetByUsernameWithUserTypeAsync(username, UserType.EndUser);
//         if (user == null)
//         {
//             _logger.LogWarning("Admin user not found: {Username}", username);
//             return null;
//         }
//         //
//         // if (!user.IsActive)
//         // {
//         //     _logger.LogWarning("Admin account is inactive: {Username}", username);
//         //     return null;
//         // }
//         //
//         // if (!_passwordService.VerifyPassword(password, user.PasswordHash))
//         // {
//         //     _logger.LogWarning("Invalid password for admin user: {Username}", username);
//         //     return null;
//         // }
//         // Additional admin specific validation
//         var userWithRoles = await _unitOfWork.UserRepository.GetWithRolesAsync(user.Id);
//         if (!userWithRoles.HasUserType(UserType.Admin))
//         {
//             _logger.LogWarning("User does not have Admin role: {Username}", username);
//             return null;
//         }
//
//         return userWithRoles;
//     }
//
//     public async Task<Dictionary<string, object>> GetAdditionalClaimsAsync(User user, CancellationToken cancellationToken = default)
//     {
//         var claims = new Dictionary<string, object>
//         {
//             { "user_type", UserType.EndUser.ToString() },
//             { "is_premium", user.HasRole("PremiumEndUser") }
//         };
//
//         // Add any EndUser specific claims
//         return claims;
//     }
// }