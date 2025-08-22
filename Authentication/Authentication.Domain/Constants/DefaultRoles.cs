using Authentication.Domain.Enums;

namespace Authentication.Domain.Constants;

public static class DefaultRoles
{
    public static readonly Dictionary<UserType, List<(string Name, string Description)>> UserTypeRoles = new()
    {
        {
            UserType.EndUser, new List<(string, string)>
            {
                (AuthenticationConstants.Roles.EndUser, "Standard end user access"),
                (AuthenticationConstants.Roles.PremiumEndUser, "Premium end user with extended features")
            }
        },
        {
            UserType.Admin, new List<(string, string)>
            {
                (AuthenticationConstants.Roles.Admin, "Standard administrative access"),
                (AuthenticationConstants.Roles.SuperAdmin, "Full system administrative access"),
                (AuthenticationConstants.Roles.SystemAdmin, "System-level administrative access")
            }
        },
        {
            UserType.Partner, new List<(string, string)>
            {
                (AuthenticationConstants.Roles.Partner, "Standard partner access"),
                (AuthenticationConstants.Roles.PartnerAdmin, "Partner administrative access"),
                (AuthenticationConstants.Roles.PartnerUser, "Partner user access")
            }
        }
    };
}