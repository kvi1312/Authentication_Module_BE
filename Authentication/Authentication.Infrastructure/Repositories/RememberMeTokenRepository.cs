using Authentication.Domain.Entities;
using Authentication.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.Repositories;

public class RememberMeTokenRepository : RepositoryBase<RememberMeToken>, IRememberMeTokenRepository
{
    public RememberMeTokenRepository(DbFactory dbFactory) : base(dbFactory)
    {
    }

    public async Task<RememberMeToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await DbSet.FirstOrDefaultAsync(rmt => rmt.TokenHash == tokenHash);
    }

    public async Task<IEnumerable<RememberMeToken>> GetByUserIdAsync(Guid userId)
    {
        return await DbSet.Where(rmt => rmt.UserId == userId).ToListAsync();
    }

    public async Task InvalidateAllByUserIdAsync(Guid userId)
    {
        var tokens = await DbSet.Where(rmt => rmt.UserId == userId && !rmt.IsUsed).ToListAsync();
        foreach (var token in tokens)
        {
            token.MarkAsUsed();
        }
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await DbSet.Where(rmt => rmt.ExpiresAt <= DateTime.UtcNow).ToListAsync();
        DbSet.RemoveRange(expiredTokens);
    }
}
