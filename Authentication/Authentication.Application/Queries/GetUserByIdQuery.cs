using Authentication.Application.Dtos.Response;
using MediatR;

namespace Authentication.Application.Queries;

public class GetUserByIdQuery : IRequest<UserManagementResponse>
{
    public Guid UserId { get; set; }
}