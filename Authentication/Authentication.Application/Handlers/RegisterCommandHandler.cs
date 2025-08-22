using Authentication.Application.Commands;
using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Interfaces;
using Authentication.Domain.Constants;
using Authentication.Domain.Entities;
using Authentication.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authentication.Application.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IJwtService jwtService,
        IMapper mapper,
        ILogger<RegisterCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Registration attempt for user: {Username}", request.Username);

            var existingUser = await _unitOfWork.UserRepository.ExistsAsync(request.Username, request.Email);
            
            if (existingUser)
            {
                _logger.LogWarning("Username or email already exists: {Username}, {Email}", request.Username, request.Email);
                return new LoginResponse 
                { 
                    Success = false, 
                    Message = "Username or email already exists" 
                };
            }

            var passwordHash = _passwordService.HashPassword(request.Password);

            var user = User.Create(
                request.Username,
                request.Email,
                passwordHash,
                request.FirstName,
                request.LastName
            );

            await _unitOfWork.UserRepository.AddAsync(user);
            var defaultRoleName = GetDefaultRoleForUserType(request.UserType);
            var role = await _unitOfWork.RolesRepository.GetByNameAsync(defaultRoleName);
            
            if (role == null)
            {
                _logger.LogError("Default role not found: {RoleName}", defaultRoleName);
                return new LoginResponse 
                { 
                    Success = false, 
                    Message = "Registration failed - default role not configured" 
                };
            }

            // Assign role to user
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                AssignedDate = DateTimeOffset.UtcNow
            };

            user.UserRoles.Add(userRole);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate tokens for immediate login
            var roles = new List<string> { role.Name };
            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var jwtId = _jwtService.GetJwtIdFromToken(accessToken);

            if (!string.IsNullOrEmpty(jwtId))
            {
                var refreshTokenEntity = RefreshToken.Create(
                    refreshToken,
                    jwtId,
                    user.Id,
                    TimeSpan.FromDays(7)
                );

                await _unitOfWork.RefreshTokensRepository.AddAsync(refreshTokenEntity);

                // Create user session if device info provided
                string? sessionId = null;
                if (!string.IsNullOrEmpty(request.DeviceInfo))
                {
                    var userSession = UserSession.Create(
                        user.Id,
                        Guid.NewGuid().ToString(),
                        TimeSpan.FromHours(24),
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

                _logger.LogInformation("Registration and auto-login successful for user: {Username}", request.Username);

                return new LoginResponse
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = refreshTokenEntity.ExpiresAt,
                    User = userDto,
                    SessionId = sessionId,
                    Message = "Registration successful"
                };
            }

            _logger.LogInformation("Registration successful for user: {Username}", request.Username);
            return new LoginResponse
            {
                Success = true,
                Message = "Registration successful. Please login."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during registration for user {Username}", request.Username);
            return new LoginResponse 
            { 
                Success = false, 
                Message = "An error occurred during registration" 
            };
        }
    }

    private string GetDefaultRoleForUserType(Authentication.Domain.Enums.UserType userType)
    {
        return userType switch
        {
            Authentication.Domain.Enums.UserType.EndUser => AuthenticationConstants.Roles.EndUser,
            Authentication.Domain.Enums.UserType.Admin => AuthenticationConstants.Roles.Admin,
            Authentication.Domain.Enums.UserType.Partner => AuthenticationConstants.Roles.Partner,
            _ => AuthenticationConstants.Roles.EndUser
        };
    }
}