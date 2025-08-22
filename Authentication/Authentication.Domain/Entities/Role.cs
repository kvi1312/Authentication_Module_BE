using Authentication.Domain.Enums;
using Authentication.Domain.Interfaces;

namespace Authentication.Domain.Entities;

public class Role : IDateTracking
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public UserType UserType { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public static Role Create(string name, string description, UserType userType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            UserType = userType,
            CreatedDate = DateTimeOffset.UtcNow
        };
    }

    public void UpdateDescription(string description) => Description = description;
}
