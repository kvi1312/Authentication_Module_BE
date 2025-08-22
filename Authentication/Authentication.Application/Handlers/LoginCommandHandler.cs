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

namespace Authentication.Application.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IAuthenticationStrategyFactory _strategyFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IAuthenticationStrategyFactory strategyFactory,
        IMapper mapper,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _strategyFactory = strategyFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
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
                request.RememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromDays(7)
            );

            await _unitOfWork.RefreshTokensRepository.AddAsync(refreshTokenEntity);

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

            var userDto = _mapper.Map<UserDto>(authenticatedUser);
            userDto.Roles = roles;
            userDto.UserType = detectedUserType!.Value; // Use auto-detected UserType (guaranteed to be non-null here)

            _logger.LogInformation("Login successful for user: {Username} as {UserType}",
                request.Username, detectedUserType);

            return new LoginResponse
            {
                Success = true,
                AccessToken = accessToken,
                ExpiresAt = refreshTokenEntity.ExpiresAt,
                User = userDto,
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
}