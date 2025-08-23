using Authentication.Domain.Entities;

namespace Authentication.Domain.Interfaces.Repositories;

public interface IRememberMeTokenRepository
{
    Task<RememberMeToken?> GetByTokenHashAsync(string tokenHash);
    Task<IEnumerable<RememberMeToken>> GetByUserIdAsync(Guid userId);
    Task AddAsync(RememberMeToken rememberMeToken);
    Task InvalidateAllByUserIdAsync(Guid userId);
    Task CleanupExpiredTokensAsync();
}
