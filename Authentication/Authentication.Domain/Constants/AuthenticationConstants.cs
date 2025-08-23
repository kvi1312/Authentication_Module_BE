namespace Authentication.Domain.Constants;

public static class AuthenticationConstants
{
    public static class Roles
    {
        public const string EndUser = "EndUser";
        public const string PremiumEndUser = "PremiumEndUser";

        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string SystemAdmin = "SystemAdmin";

        public const string Partner = "Partner";
        public const string PartnerAdmin = "PartnerAdmin";
        public const string PartnerUser = "PartnerUser";
    }
}