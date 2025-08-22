using Authentication.Domain.Entities;

namespace Authentication.Domain.Services;

public interface ITokenValidationService
{
    Task<bool> IsRefreshTokenValidAsync(string token);
    Task<bool> IsRememberMeTokenValidAsync(string tokenHash, Guid userId);
    Task<RefreshToken?> GetValidRefreshTokenAsync(string token);
    Task<RememberMeToken?> GetValidRememberMeTokenAsync(string tokenHash, Guid userId);
}