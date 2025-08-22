namespace Authentication.Domain.Configurations;

public class CookieSettings
{
    public const string SectionName = "CookieSettings";
    
    public string Domain { get; set; } = default!;
    public string Path { get; set; } = "/";
    public bool HttpOnly { get; set; } = true;
    public bool Secure { get; set; } = true;
    public string SameSite { get; set; } = "Strict";
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public int RememberMeExpiryDays { get; set; } = 30;
}