using Authentication.Application.Commands;
using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Interfaces;
using Authentication.Application.Strategies;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Authentication.Application.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IAuthenticationStrategyFactory _strategyFactory;
    private readonly ITokenConfigService _tokenConfigService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IAuthenticationStrategyFactory strategyFactory,
        ITokenConfigService tokenConfigService,
        IPasswordService passwordService,
        IMapper mapper,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _strategyFactory = strategyFactory;
        _tokenConfigService = tokenConfigService;
        _passwordService = passwordService;
        _tokenConfigService = tokenConfigService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username} (auto-detecting UserType)", request.Username);

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

            var accessToken = _jwtService.GenerateAccessToken(authenticatedUser, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var jwtId = _jwtService.GetJwtIdFromToken(accessToken);

            if (string.IsNullOrEmpty(jwtId))
            {
                _logger.LogError("Failed to extract JWT ID from access token for user: {Username}", request.Username);
                return new LoginResponse
                {
                    Success = false,
                    Message = "Token generation failed"
                };
            }

            var refreshTokenEntity = RefreshToken.Create(
                refreshToken,
                jwtId,
                authenticatedUser.Id,
                GetRefreshTokenExpiry(false) // Always use standard refresh token duration
            );

            _logger.LogInformation("Creating refresh token for user {UserId}, Token: {Token}, ExpiresAt: {ExpiresAt}",
                authenticatedUser.Id, refreshToken, refreshTokenEntity.ExpiresAt);

            await _unitOfWork.RefreshTokensRepository.AddAsync(refreshTokenEntity);

            // Create RememberMeToken if requested
            string? rememberMeToken = null;
            if (request.RememberMe)
            {
                rememberMeToken = GenerateRememberMeToken();
                var rememberMeTokenHash = _passwordService.HashPassword(rememberMeToken);
                var rememberMeEntity = RememberMeToken.Create(
                    authenticatedUser.Id,
                    rememberMeTokenHash,
                    GetRememberMeTokenExpiry()
                );

                _logger.LogInformation("Creating remember me token for user {UserId}, ExpiresAt: {ExpiresAt}",
                    authenticatedUser.Id, rememberMeEntity.ExpiresAt);

                await _unitOfWork.RememberMeTokensRepository.AddAsync(rememberMeEntity);
            }

            _logger.LogInformation("Refresh token added to repository, saving changes...");

            string? sessionId = null;
            if (!string.IsNullOrEmpty(request.DeviceInfo))
            {
                var userSession = UserSession.Create(
                    authenticatedUser.Id,
                    Guid.NewGuid().ToString(),
                    TimeSpan.FromHours(24),
                    request.DeviceInfo,
                    request.IpAddress
                );

                await _unitOfWork.UserSessionsRepository.AddAsync(userSession);
                sessionId = userSession.SessionId;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Database changes saved successfully. Refresh token should now be in database.");

            var userDto = _mapper.Map<UserDto>(authenticatedUser);
            userDto.Roles = roles;
            userDto.UserType = detectedUserType!.Value;

            _logger.LogInformation("Login successful for user: {Username} as {UserType}",
                request.Username, detectedUserType);

            return new LoginResponse
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RememberMeToken = rememberMeToken,

                AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiryTime(),
                RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
                RememberMeTokenExpiresAt = request.RememberMe ? DateTime.UtcNow.Add(GetRememberMeTokenExpiry()) : null,

                User = userDto,
                SessionId = sessionId,
                Message = "Login successful"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login for user {Username}", request.Username);
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login"
            };
        }
    }

    private TimeSpan GetRefreshTokenExpiry(bool rememberMe)
    {
        var config = _tokenConfigService.GetCurrentConfig();

        if (rememberMe)
        {
            return TimeSpan.FromDays(config.RememberMeTokenExpiryDays);
        }
        else
        {
            return TimeSpan.FromDays(config.RefreshTokenExpiryDays);
        }
    }

    private TimeSpan GetRememberMeTokenExpiry()
    {
        var config = _tokenConfigService.GetCurrentConfig();
        return TimeSpan.FromDays(config.RememberMeTokenExpiryDays);
    }

    private string GenerateRememberMeToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
