using Authentication.Domain.Enums;

namespace Authentication.Domain.Constants;

public static class DefaultRoles
{
    public static readonly Dictionary<UserType, List<(string Name, string Description)>> UserTypeRoles = new()
    {
        {
            UserType.EndUser, new List<(string, string)>
            {
                ("Customer", "Standard customer access"),
                ("Guest", "Guest user access")
            }
        },
        {
            UserType.Admin, new List<(string, string)>
            {
                ("Admin", "Standard administrative access"),
                ("SuperAdmin", "Full system administrative access"),
            }
        },
        {
            UserType.Partner, new List<(string, string)>
            {
                ("Partner", "Standard partner access"),
                ("Manager", "Management level access"),
                ("Employee", "Employee level access")
            }
        }
    };
}