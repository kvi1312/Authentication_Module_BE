using Authentication.Domain.Enums;

namespace Authentication.Application.Extensions;

public static class RoleExtensions
{
    public static RoleType ToRoleType(this string roleName)
    {
        return roleName.ToLower() switch
        {
            "customer" => RoleType.Customer,
            "admin" => RoleType.Admin,
            "manager" => RoleType.Manager,
            "superadmin" => RoleType.SuperAdmin,
            "employee" => RoleType.Employee,
            "partner" => RoleType.Partner,
            "guest" => RoleType.Guest,
            _ => RoleType.Customer
        };
    }

    public static List<RoleType> ToRoleTypes(this IEnumerable<string> roleNames)
    {
        return roleNames.Select(ToRoleType).ToList();
    }

    public static string ToRoleName(this RoleType roleType)
    {
        return roleType switch
        {
            RoleType.Customer => "Customer",
            RoleType.Admin => "Admin",
            RoleType.Manager => "Manager",
            RoleType.SuperAdmin => "SuperAdmin",
            RoleType.Employee => "Employee",
            RoleType.Partner => "Partner",
            RoleType.Guest => "Guest",
            _ => "Customer"
        };
    }

    public static List<string> ToRoleNames(this IEnumerable<RoleType> roleTypes)
    {
        return roleTypes.Select(ToRoleName).ToList();
    }
}
