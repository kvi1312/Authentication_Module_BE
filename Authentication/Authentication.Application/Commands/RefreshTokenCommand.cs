using Authentication.Application.Dtos.Response;
using MediatR;

namespace Authentication.Application.Commands;

public class RefreshTokenCommand : IRequest<RefreshTokenResponse>
{
    public string RefreshToken { get; set; } = default!;
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; }
}