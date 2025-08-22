using Authentication.Application.Dtos.Response;
using MediatR;

namespace Authentication.Application.Commands;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool RememberMe { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}