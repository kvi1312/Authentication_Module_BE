namespace Authentication.Domain.Configurations;

public class SecuritySettings
{
    public const string SectionName = "SecuritySettings";
    
    public bool EnableRateLimiting { get; set; } = true;
    public int LoginAttemptWindow { get; set; } = 15;
    public int MaxLoginAttempts { get; set; } = 5;
    public bool EnableIPWhitelisting { get; set; } = false;
    public List<string> WhitelistedIPs { get; set; } = [];
    public bool RequireHttps { get; set; } = true;
    public bool EnableCors { get; set; } = true;
    public List<string> AllowedOrigins { get; set; } = [];
    public bool LogSecurityEvents { get; set; } = true;
    public bool EnableTwoFactorAuth { get; set; } = false;
}