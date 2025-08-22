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
}