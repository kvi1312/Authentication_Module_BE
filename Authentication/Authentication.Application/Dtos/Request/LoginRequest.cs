namespace Authentication.Application.Dtos.Request;

public class LoginRequest
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool RememberMe { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}