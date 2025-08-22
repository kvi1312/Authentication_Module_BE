using Authentication.Domain.Entities;

namespace Authentication.Domain.Interfaces.Repositories;

public interface IUserSessionRepository
{
    Task<UserSession?> GetBySessionIdAsync(string sessionId);
    Task<IEnumerable<UserSession>> GetActiveByUserIdAsync(Guid userId);
    Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid userId);
    Task DeactivateAllByUserIdAsync(Guid userId);
    Task CleanupExpiredSessionsAsync();
}
