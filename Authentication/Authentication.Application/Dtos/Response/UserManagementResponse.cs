using Authentication.Application.Dtos;

namespace Authentication.Application.Dtos.Response;

public class UserManagementResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public UserDto? User { get; set; }
}

public class UsersListResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public List<UserDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class UpdateUserProfileResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public UserDto? User { get; set; }
}
