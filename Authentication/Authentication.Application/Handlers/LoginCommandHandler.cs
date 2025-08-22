using Authentication.Application.Commands;
using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Interfaces;
using Authentication.Application.Strategies;
using Authentication.Domain.Entities;
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
            _logger.LogInformation("Login attempt for user: {Username} with UserType: {UserType}",
                request.Username, request.UserType);

            var strategy = _strategyFactory.GetStrategy(request.UserType);

            var user = await strategy.ValidateAsync(request.Username, request.Password, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Login failed for user: {Username}", request.Username);
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid credentials or user type mismatch"
                };
            }

            user.UpdateLastLogin();
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            var accessToken = _jwtService.GenerateAccessToken(user, roles);
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
                user.Id,
                request.RememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromDays(7)
            );

            await _unitOfWork.RefreshTokensRepository.AddAsync(refreshTokenEntity);

            // Create user session if enabled
            string? sessionId = null;
            if (!string.IsNullOrEmpty(request.DeviceInfo))
            {
                var userSession = UserSession.Create(
                    user.Id,
                    Guid.NewGuid().ToString(),
                    TimeSpan.FromHours(24), // Session validity
                    request.DeviceInfo,
                    request.IpAddress
                );

                await _unitOfWork.UserSessionsRepository.AddAsync(userSession);
                sessionId = userSession.SessionId;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles;
            userDto.UserType = request.UserType;

            _logger.LogInformation("Login successful for user: {Username}", request.Username);

            return new LoginResponse
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = refreshTokenEntity.ExpiresAt,
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
}