namespace Authentication.Domain.Configurations;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string SecretKey { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpiryMinutes { get; set; } = 5;
    public double RefreshTokenExpiryDays { get; set; } = 0.25;
    public double RememberMeTokenExpiryDays { get; set; } = 1;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public int ClockSkewSeconds { get; set; } = 0;

    public TimeSpan GetRefreshTokenExpiry() => TimeSpan.FromDays(RefreshTokenExpiryDays);
    public TimeSpan GetRememberMeTokenExpiry() => TimeSpan.FromDays(RememberMeTokenExpiryDays);
    public TimeSpan GetAccessTokenExpiry() => TimeSpan.FromMinutes(ExpiryMinutes);
}