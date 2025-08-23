using Authentication.Domain.Entities;

namespace Authentication.Domain.Interfaces.Repositories;

public interface IRefreshTokenRepository : IRepositoryBase<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken?> GetByJwtIdAsync(string jwtId);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId);
    Task RevokeAllByUserIdAsync(Guid userId);
    Task CleanupExpiredTokensAsync();
}
