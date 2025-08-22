namespace Authentication.Application.Dtos.Request;

public class LogoutRequest
{
    public string? RefreshToken { get; set; }
    public string? AccessToken { get; set; }
    public bool LogoutFromAllDevices { get; set; } = false;
}