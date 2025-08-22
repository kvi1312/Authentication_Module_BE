namespace Authentication.Domain.Configurations;

public class PasswordPolicySettings
{
    public const string SectionName = "PasswordPolicySettings";
    public int MinimumLength { get; set; } = 8;
    public int MaximumLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;
    public int MinimumUniqueCharacters { get; set; } = 4;
    public bool PreventCommonPasswords { get; set; } = true;
    public int PasswordHistoryCount { get; set; } = 5;
    public int PasswordExpiryDays { get; set; } = 90;
}