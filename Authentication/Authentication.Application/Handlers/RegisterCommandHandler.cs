using Authentication.Application.Commands;
using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Extensions;
using Authentication.Application.Interfaces;
using Authentication.Domain.Constants;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authentication.Application.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
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

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Registration attempt for user: {Username}", request.Username);

            var existingUser = await _unitOfWork.UserRepository.ExistsAsync(request.Username, request.Email);

            if (existingUser)
            {
                _logger.LogWarning("Username or email already exists: {Username}, {Email}", request.Username, request.Email);
                return new RegisterResponse
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

            // Public registration always creates EndUser
            var defaultRoleName = GetDefaultRoleForUserType(UserType.EndUser);
            var role = await _unitOfWork.RolesRepository.GetByNameAsync(defaultRoleName);

            if (role == null)
            {
                _logger.LogError("Default role not found: {RoleName}", defaultRoleName);
                return new RegisterResponse
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

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = new List<string> { role.Name }.ToRoleTypes();
            userDto.UserType = UserType.EndUser; // Public registration always creates EndUser

            _logger.LogInformation("Registration successful for user: {Username} as EndUser", request.Username);

            return new RegisterResponse
            {
                Success = true,
                Message = "Registration successful. You can now login.",
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during registration for user {Username}", request.Username);
            return new RegisterResponse
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
            Authentication.Domain.Enums.UserType.EndUser => "Customer",
            Authentication.Domain.Enums.UserType.Admin => "Admin",
            Authentication.Domain.Enums.UserType.Partner => "Partner",
            _ => "Customer"
        };
    }
}