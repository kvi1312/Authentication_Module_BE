using Authentication.Domain.Interfaces;

namespace Authentication.Domain.Entities;

public class RefreshToken : IDateTracking
{
    public Guid Id { get; set; }
    public string Token { get; set; } = default!;
    public string JwtId { get; set; } = default!;
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }

    public virtual User User { get; set; } = default!;
    public static RefreshToken Create(string token, string jwtId, Guid userId, TimeSpan validity)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        if (string.IsNullOrWhiteSpace(jwtId))
            throw new ArgumentException("JWT ID cannot be empty", nameof(jwtId));

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            JwtId = jwtId,
            UserId = userId,
            CreatedDate = DateTimeOffset.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(validity),
            IsUsed = false,
            IsRevoked = false
        };
    }

    public void MarkAsUsed() => IsUsed = true;
    public void MarkAsRevoked() => IsRevoked = true;
    public bool IsValid() => !IsUsed && !IsRevoked && DateTime.UtcNow < ExpiresAt;
}
