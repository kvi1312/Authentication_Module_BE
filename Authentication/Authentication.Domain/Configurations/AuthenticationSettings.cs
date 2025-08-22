namespace Authentication.Domain.Configurations;

public class AuthenticationSettings
{
    public const string SectionName = "AuthenticationSettings";
    
    public bool AllowRememberMe { get; set; } = true;
    public int RememberMeExpiryDays { get; set; } = 30;
    public bool RequireEmailConfirmation { get; set; } = false;
    public bool EnableSessionManagement { get; set; } = true;
    public int MaxConcurrentSessions { get; set; } = 5;
    public bool EnableMultipleDeviceLogin { get; set; } = true;
    public int SessionTimeoutMinutes { get; set; } = 30;
    public bool LogFailedAttempts { get; set; } = true;
    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 30;
    public bool EnablePasswordPolicy { get; set; } = true;
}