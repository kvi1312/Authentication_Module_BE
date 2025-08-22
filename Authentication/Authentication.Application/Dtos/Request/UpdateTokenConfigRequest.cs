namespace Authentication.Application.Dtos.Request;

public class UpdateTokenConfigRequest
{
    public int? AccessTokenExpiryMinutes { get; set; }
    public double? RefreshTokenExpiryDays { get; set; }
    public double? RememberMeTokenExpiryDays { get; set; }
}
