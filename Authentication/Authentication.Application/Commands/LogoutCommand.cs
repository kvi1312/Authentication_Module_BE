using MediatR;

namespace Authentication.Application.Commands;

public class LogoutCommand : IRequest<bool>
{
    public string? RefreshToken { get; set; }
    public string? AccessToken { get; set; }
    public string? UserId { get; set; }
    public bool LogoutFromAllDevices { get; set; }
}