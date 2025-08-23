using Authentication.Domain.Enums;

namespace Authentication.Application.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName => $"{FirstName} {LastName}";
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public List<RoleType> Roles { get; set; } = new();
    public UserType UserType { get; set; }
}