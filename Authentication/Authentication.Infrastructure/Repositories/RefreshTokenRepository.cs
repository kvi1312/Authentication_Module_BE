using Authentication.Domain.Entities;
using Authentication.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.Repositories;

public class RefreshTokenRepository : RepositoryBase<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(DbFactory dbFactory) : base(dbFactory)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await DbSet.FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<RefreshToken?> GetByJwtIdAsync(string jwtId)
    {
        return await DbSet.FirstOrDefaultAsync(rt => rt.JwtId == jwtId);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId)
    {
        return await DbSet.Where(rt => rt.UserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId)
    {
        return await DbSet.Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                          .ToListAsync();
    }

    public async Task RevokeAllByUserIdAsync(Guid userId)
    {
        var tokens = await DbSet.Where(rt => rt.UserId == userId && !rt.IsRevoked).ToListAsync();
        foreach (var token in tokens)
        {
            token.MarkAsRevoked();
        }
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await DbSet.Where(rt => rt.ExpiresAt <= DateTime.UtcNow).ToListAsync();
        DbSet.RemoveRange(expiredTokens);
    }
}
