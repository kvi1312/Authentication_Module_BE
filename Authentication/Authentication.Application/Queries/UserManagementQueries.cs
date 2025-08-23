using Authentication.Application.Dtos.Response;
using Authentication.Domain.Enums;
using MediatR;

namespace Authentication.Application.Queries;

public class GetUsersQuery : IRequest<UsersListResponse>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public UserType? UserType { get; set; }
    public RoleType? RoleFilter { get; set; }
}
