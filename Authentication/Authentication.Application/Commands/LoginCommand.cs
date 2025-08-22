using Authentication.Application.Dtos.Response;
using Authentication.Domain.Enums;
using MediatR;

namespace Authentication.Application.Commands;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public UserType  UserType { get; set; }
    public bool RememberMe { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}