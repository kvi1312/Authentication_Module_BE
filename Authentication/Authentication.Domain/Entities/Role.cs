using Authentication.Domain.Interfaces;

namespace Authentication.Domain.Entities;

public class Role : IDateTracking
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }
}
