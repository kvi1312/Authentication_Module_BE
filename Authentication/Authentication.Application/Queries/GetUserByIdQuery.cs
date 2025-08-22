using Authentication.Application.Dtos;
using MediatR;

namespace Authentication.Application.Queries;

public class GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid UserId { get; set; }
}