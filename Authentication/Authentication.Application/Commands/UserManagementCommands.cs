using Authentication.Application.Dtos.Response;
using Authentication.Domain.Enums;
using MediatR;

namespace Authentication.Application.Commands;

public class AddUserRoleCommand : IRequest<UserManagementResponse>
{
    public Guid UserId { get; set; }
    public List<RoleType> RolesToAdd { get; set; } = new();
}

public class RemoveUserRoleCommand : IRequest<UserManagementResponse>
{
    public Guid UserId { get; set; }
    public List<RoleType> RolesToRemove { get; set; } = new();
}

public class UpdateUserProfileCommand : IRequest<UpdateUserProfileResponse>
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}
