using Authentication.Domain.Entities;
using Authentication.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.Repositories;

public class UserSessionRepository : RepositoryBase<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(DbFactory dbFactory) : base(dbFactory)
    {
    }

    public async Task<UserSession?> GetBySessionIdAsync(string sessionId)
    {
        return await DbSet.FirstOrDefaultAsync(us => us.SessionId == sessionId);
    }

    public async Task<IEnumerable<UserSession>> GetActiveByUserIdAsync(Guid userId)
    {
        return await DbSet.Where(us => us.UserId == userId && us.IsActive && us.ExpiresAt > DateTime.UtcNow)
                          .ToListAsync();
    }

    public async Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid userId)
    {
        return await DbSet.Where(us => us.UserId == userId).ToListAsync();
    }

    public async Task DeactivateAllByUserIdAsync(Guid userId)
    {
        var sessions = await DbSet.Where(us => us.UserId == userId && us.IsActive).ToListAsync();
        foreach (var session in sessions)
        {
            session.Deactivate();
        }
    }

    public async Task CleanupExpiredSessionsAsync()
    {
        var expiredSessions = await DbSet.Where(us => us.ExpiresAt <= DateTime.UtcNow).ToListAsync();
        DbSet.RemoveRange(expiredSessions);
    }
}
