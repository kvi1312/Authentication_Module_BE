namespace Authentication.Application.Dtos.Response;

public class RefreshTokenResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
}