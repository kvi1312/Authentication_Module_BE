using Authentication.Application.Dtos;

namespace Authentication.Application.Dtos.Response;

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? RememberMeToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserDto? User { get; set; }
    public string? SessionId { get; set; }
}