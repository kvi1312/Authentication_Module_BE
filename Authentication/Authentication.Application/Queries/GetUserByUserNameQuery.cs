using Authentication.Application.Dtos;
using Authentication.Domain.Enums;
using MediatR;

namespace Authentication.Application.Queries;

public class GetUserByUserNameQuery : IRequest<UserDto?>
{
    public string UserName { get; set; } = default!;
    public UserType UserType { get; set; }
}