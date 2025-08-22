namespace Authentication.Application.Dtos.Response;

public class TokenConfigResponse
{
    public int AccessTokenExpiryMinutes { get; set; }
    public double RefreshTokenExpiryDays { get; set; }
    public double RememberMeTokenExpiryDays { get; set; }
    public string AccessTokenExpiryDisplay { get; set; } = default!;
    public string RefreshTokenExpiryDisplay { get; set; } = default!;
    public string RememberMeTokenExpiryDisplay { get; set; } = default!;
}
