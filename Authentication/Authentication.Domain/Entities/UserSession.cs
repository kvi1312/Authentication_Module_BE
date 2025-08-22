using Authentication.Domain.Interfaces;

namespace Authentication.Domain.Entities;

public class UserSession : IDateTracking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SessionId { get; set; } = default!;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }

    public virtual User User { get; set; } = default!;

    public static UserSession Create(Guid userId, string sessionId, TimeSpan validity,
        string? deviceInfo = null, string? ipAddress = null)
    {
        return new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SessionId = sessionId,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            ExpiresAt = DateTime.UtcNow.Add(validity),
            IsActive = true,
            CreatedDate = DateTimeOffset.UtcNow
        };
    }
    public void Deactivate() => IsActive = false;
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt || !IsActive;
}

