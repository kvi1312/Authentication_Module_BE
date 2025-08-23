using Authentication.Application.Commands;
using Authentication.Application.Dtos.Response;
using Authentication.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authentication.Application.Handlers;

public class UserManagementCommandHandler :
    IRequestHandler<AddUserRoleCommand, UserManagementResponse>,
    IRequestHandler<RemoveUserRoleCommand, UserManagementResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserManagementCommandHandler> _logger;

    public UserManagementCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UserManagementCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserManagementResponse> Handle(AddUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Adding roles to user {UserId}: {Roles}", request.UserId, string.Join(", ", request.RolesToAdd));

            var user = await _unitOfWork.UserRepository.GetWithRolesAsync(request.UserId);
            if (user == null)
            {
                return new UserManagementResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            if (!user.IsActive)
            {
                return new UserManagementResponse
                {
                    Success = false,
                    Message = "User is not active"
                };
            }

            var rolesToAdd = await _unitOfWork.RolesRepository.GetByRoleTypesAsync(request.RolesToAdd);
            if (!rolesToAdd.Any())
            {
                return new UserManagementResponse
                {
                    Success = false,
                    Message = "No valid roles found"
                };
            }

            if (rolesToAdd.Count() != request.RolesToAdd.Count)
            {
                var foundRoleTypes = rolesToAdd.Select(r => r.Name).ToList();
                var requestedRoleTypes = request.RolesToAdd.Select(r => r.ToString()).ToList();
                var missingRoles = requestedRoleTypes.Except(foundRoleTypes).ToList();

                return new UserManagementResponse
                {
                    Success = false,
                    Message = $"Some roles not found: {string.Join(", ", missingRoles)}"
                };
            }

            var addedRoles = 0;
            foreach (var role in rolesToAdd)
            {
                var existingUserRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
                if (existingUserRole == null)
                {
                    await _unitOfWork.UserRepository.AddUserRoleAsync(user.Id, role.Id);
                    addedRoles++;
                    _logger.LogInformation("Added role {RoleName} to user {UserId}", role.Name, user.Id);
                }
                else
                {
                    _logger.LogInformation("User {UserId} already has role {RoleName}", user.Id, role.Name);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UserManagementResponse
            {
                Success = true,
                Message = addedRoles > 0 ? "Roles added successfully" : "No new roles were added (user already had all requested roles)"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding roles to user {UserId}", request.UserId);
            return new UserManagementResponse
            {
                Success = false,
                Message = "An error occurred while adding roles"
            };
        }
    }

    public async Task<UserManagementResponse> Handle(RemoveUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Removing roles from user {UserId}: {Roles}", request.UserId, string.Join(", ", request.RolesToRemove));

            var user = await _unitOfWork.UserRepository.GetWithRolesAsync(request.UserId);
            if (user == null)
            {
                return new UserManagementResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var rolesToRemove = await _unitOfWork.RolesRepository.GetByRoleTypesAsync(request.RolesToRemove);
            if (!rolesToRemove.Any())
            {
                return new UserManagementResponse
                {
                    Success = false,
                    Message = "No valid roles found"
                };
            }

            var removedRoles = 0;
            foreach (var role in rolesToRemove)
            {
                var existingUserRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
                if (existingUserRole != null)
                {
                    await _unitOfWork.UserRepository.RemoveUserRoleAsync(user.Id, role.Id);
                    removedRoles++;
                    _logger.LogInformation("Removed role {RoleName} from user {UserId}", role.Name, user.Id);
                }
                else
                {
                    _logger.LogInformation("User {UserId} does not have role {RoleName}", user.Id, role.Name);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UserManagementResponse
            {
                Success = true,
                Message = removedRoles > 0 ? "Roles removed successfully" : "No roles were removed (user didn't have the specified roles)"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing roles from user {UserId}", request.UserId);
            return new UserManagementResponse
            {
                Success = false,
                Message = "An error occurred while removing roles"
            };
        }
    }
}
