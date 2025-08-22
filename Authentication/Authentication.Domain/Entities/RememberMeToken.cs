using Authentication.Domain.Interfaces;

namespace Authentication.Domain.Entities;

public class RememberMeToken : IDateTracking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = default!; // Store hashed version
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }

    public virtual User User { get; set; } = default!;

    public static RememberMeToken Create(Guid userId, string tokenHash, TimeSpan validity)
    {
        return new RememberMeToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.Add(validity),
            IsUsed = false,
            CreatedDate = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsUsed() => IsUsed = true;

    public bool IsValid() => !IsUsed && DateTime.UtcNow < ExpiresAt;
}
