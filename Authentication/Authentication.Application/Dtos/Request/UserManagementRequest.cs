using Authentication.Domain.Enums;

namespace Authentication.Application.Dtos.Request;

public class AddUserRoleRequest
{
    public Guid UserId { get; set; }
    public List<RoleType> RolesToAdd { get; set; } = new();
}

public class RemoveUserRoleRequest
{
    public Guid UserId { get; set; }
    public List<RoleType> RolesToRemove { get; set; } = new();
}

public class GetUsersRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public UserType? UserType { get; set; }
    public RoleType? RoleFilter { get; set; }
}
