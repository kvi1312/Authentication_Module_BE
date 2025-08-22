using Authentication.Domain.Enums;

namespace Authentication.Application.Dtos;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public UserType UserType { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}