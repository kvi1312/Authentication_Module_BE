using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Extensions;
using Authentication.Application.Interfaces;
using Authentication.Application.Strategies;
using Authentication.Domain.Configurations;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authentication.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IAuthenticationStrategyFactory _strategyFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IOptionsMonitor<JwtSettings> _jwtOptions;

    public AuthenticationService(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordService passwordService,
        IAuthenticationStrategyFactory strategyFactory,
        IMapper mapper,
        ILogger<AuthenticationService> logger,
        IOptionsMonitor<JwtSettings> jwtOptions)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _strategyFactory = strategyFactory;
        _mapper = mapper;
        _logger = logger;
        _jwtOptions = jwtOptions;
    }

    private JwtSettings GetCurrentSettings() => TokenConfigService.GetRuntimeSettings(_jwtOptions);

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username} (auto-detecting UserType)", request.Username);

            // Auto-detect UserType by trying all strategies
            User? authenticatedUser = null;
            UserType? detectedUserType = null;

            foreach (var strategy in _strategyFactory.GetAllStrategies())
            {
                var user = await strategy.ValidateAsync(request.Username, request.Password, cancellationToken);
                if (user != null)
                {
                    authenticatedUser = user;
                    detectedUserType = strategy.UserType;
                    _logger.LogInformation("User {Username} authenticated as {UserType}",
                        request.Username, strategy.UserType);
                    break;
                }
            }

            if (authenticatedUser == null)
            {
                _logger.LogWarning("Login failed for user: {Username} - Invalid credentials", request.Username);
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }

            authenticatedUser.UpdateLastLogin();
            var roles = authenticatedUser.UserRoles.Select(ur => ur.Role.Name).ToList();

            var settings = GetCurrentSettings();
            var accessToken = _jwtService.GenerateAccessToken(authenticatedUser, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var jwtId = _jwtService.GetJwtIdFromToken(accessToken);

            if (string.IsNullOrEmpty(jwtId))
            {
                _logger.LogError("Failed to extract JWT ID from access token for user: {Username}", request.Username);
                return new LoginResponse
                {
                    Success = false,
                    Message = "Authentication failed"
                };
            }

            var refreshTokenEntity = RefreshToken.Create(
                refreshToken,
                jwtId,
                authenticatedUser.Id,
                request.RememberMe ? settings.GetRememberMeTokenExpiry() : settings.GetRefreshTokenExpiry()
            );

            await _unitOfWork.RefreshTokensRepository.AddAsync(refreshTokenEntity);

            string? rememberMeToken = null;
            if (request.RememberMe)
            {
                rememberMeToken = GenerateRememberMeToken();
                var rememberMeTokenHash = _passwordService.HashPassword(rememberMeToken);
                var rememberMeEntity = RememberMeToken.Create(
                    authenticatedUser.Id,
                    rememberMeTokenHash,
                    settings.GetRememberMeTokenExpiry()
                );
                await _unitOfWork.RememberMeTokensRepository.AddAsync(rememberMeEntity);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {Username} logged in successfully as {UserType}", request.Username, detectedUserType);

            var userDto = _mapper.Map<UserDto>(authenticatedUser);
            userDto.UserType = detectedUserType!.Value;
            userDto.Roles = roles.ToRoleTypes();

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTime.UtcNow.Add(settings.GetAccessTokenExpiry()),
                RefreshTokenExpiresAt = DateTime.UtcNow.Add(settings.GetRefreshTokenExpiry()),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during auto-detect login for user: {Username}", request.Username);
            return new LoginResponse { Success = false, Message = "An error occurred during login" };
        }
    }

    public async Task<bool> LogoutAsync(LogoutRequest request, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                var refreshToken = await _unitOfWork.RefreshTokensRepository.GetByTokenAsync(request.RefreshToken);
                if (refreshToken != null)
                {
                    refreshToken.MarkAsRevoked();
                    userId ??= refreshToken.UserId;
                }
            }

            if (!string.IsNullOrEmpty(request.AccessToken))
            {
                var jwtId = _jwtService.GetJwtIdFromToken(request.AccessToken);
                if (!string.IsNullOrEmpty(jwtId))
                {
                    var expiry = _jwtService.GetTokenExpirationDate(request.AccessToken);
                    await _jwtService.BlacklistTokenAsync(jwtId, expiry);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} logged out successfully", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var storedRefreshToken = await _unitOfWork.RefreshTokensRepository.GetByTokenAsync(request.RefreshToken);

            if (storedRefreshToken == null || !storedRefreshToken.IsValid())
            {
                _logger.LogWarning("Invalid or expired refresh token");
                return new RefreshTokenResponse { Success = false, Message = "Invalid or expired refresh token" };
            }

            var user = await _unitOfWork.UserRepository.GetWithRolesAsync(storedRefreshToken.UserId);

            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("User not found or inactive for refresh token");
                return new RefreshTokenResponse { Success = false, Message = "User not found or inactive" };
            }

            storedRefreshToken.MarkAsRevoked();

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var jwtId = _jwtService.GetJwtIdFromToken(newAccessToken);

            if (string.IsNullOrEmpty(jwtId))
            {
                _logger.LogError("Failed to extract JWT ID from new access token");
                return new RefreshTokenResponse { Success = false, Message = "Token generation failed" };
            }

            var settings = GetCurrentSettings();
            var newRefreshTokenEntity = RefreshToken.Create(
                newRefreshToken,
                jwtId,
                user.Id,
                settings.GetRefreshTokenExpiry()
            );

            await _unitOfWork.RefreshTokensRepository.AddAsync(newRefreshTokenEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Token refresh successful for user: {UserId}", user.Id);

            return new RefreshTokenResponse
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.Add(settings.GetAccessTokenExpiry()),
                RefreshTokenExpiresAt = newRefreshTokenEntity.ExpiresAt,
                Message = "Token refreshed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token refresh");
            return new RefreshTokenResponse { Success = false, Message = "An error occurred during token refresh" };
        }
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _unitOfWork.UserRepository.ExistsAsync(request.Username, request.Email))
            {
                return new LoginResponse { Success = false, Message = "Username or email already exists" };
            }

            var passwordHash = _passwordService.HashPassword(request.Password);

            var user = User.Create(request.Username, request.Email, passwordHash, request.FirstName, request.LastName);
            await _unitOfWork.UserRepository.AddAsync(user);

            var defaultRoleName = GetDefaultRoleForUserType(UserType.EndUser);
            var role = await _unitOfWork.RolesRepository.GetByNameAsync(defaultRoleName);

            if (role != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedDate = DateTimeOffset.UtcNow
                };
                user.UserRoles.Add(userRole);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {Username} registered successfully", request.Username);

            var loginRequest = new LoginRequest
            {
                Username = request.Username,
                Password = request.Password,
                RememberMe = false
            };

            return await LoginAsync(loginRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during registration for user: {Username}", request.Username);
            return new LoginResponse { Success = false, Message = "An error occurred during registration" };
        }
    }

    public async Task<bool> ValidateTokenAsync(string token, string tokenType, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (tokenType.ToLower())
            {
                case "access":
                case "bearer":
                    if (_jwtService.IsTokenExpired(token))
                        return false;

                    var jwtId = _jwtService.GetJwtIdFromToken(token);
                    return !string.IsNullOrEmpty(jwtId) && !await _jwtService.IsTokenBlacklistedAsync(jwtId);

                case "refresh":
                    var refreshToken = await _unitOfWork.RefreshTokensRepository.GetByTokenAsync(token);
                    return refreshToken?.IsValid() == true;

                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token of type: {TokenType}", tokenType);
            return false;
        }
    }

    private string GetDefaultRoleForUserType(UserType userType)
    {
        return userType switch
        {
            UserType.EndUser => "EndUser",
            UserType.Admin => "Admin",
            UserType.Partner => "Partner",
            _ => "EndUser"
        };
    }

    private string GenerateRememberMeToken()
    {
        var randomBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}