namespace Authentication.Domain.Constants;

public static class AuthenticationConstants
{
    public static class Roles
    {
        // EndUser Roles
        public const string EndUser = "EndUser";
        public const string PremiumEndUser = "PremiumEndUser";
        
        // Admin Roles
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string SystemAdmin = "SystemAdmin";
        
        // Partner Roles
        public const string Partner = "Partner";
        public const string PartnerAdmin = "PartnerAdmin";
        public const string PartnerUser = "PartnerUser";
    }

    public static class Claims
    {
        public const string UserId = "user_id";
        public const string UserType = "user_type";
        public const string SessionId = "session_id";
        public const string DeviceId = "device_id";
        public const string LastLoginAt = "last_login_at";
    }

    public static class Cookies
    {
        public const string RefreshToken = "refresh_token";
        public const string RememberMe = "remember_me";
        public const string SessionId = "session_id";
    }

    public static class TokenTypes
    {
        public const string Bearer = "Bearer";
        public const string Refresh = "Refresh";
        public const string RememberMe = "RememberMe";
    }

    public static class ErrorCodes
    {
        public const string InvalidCredentials = "AUTH001";
        public const string UserInactive = "AUTH002";
        public const string InvalidUserType = "AUTH003";
        public const string TokenExpired = "AUTH004";
        public const string TokenRevoked = "AUTH005";
        public const string InvalidToken = "AUTH006";
        public const string AccountLocked = "AUTH007";
        public const string PasswordExpired = "AUTH008";
        public const string SessionExpired = "AUTH009";
        public const string DuplicateUsername = "AUTH010";
        public const string DuplicateEmail = "AUTH011";
    }
}