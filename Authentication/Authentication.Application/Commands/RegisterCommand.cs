using Authentication.Application.Dtos.Response;
using Authentication.Domain.Enums;
using MediatR;

namespace Authentication.Application.Commands;

public class RegisterCommand : IRequest<RegisterResponse>
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public UserType UserType { get; set; } = UserType.EndUser;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}