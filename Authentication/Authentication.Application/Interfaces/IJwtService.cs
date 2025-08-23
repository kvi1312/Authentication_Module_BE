using Authentication.Domain.Entities;
using System.Security.Claims;

namespace Authentication.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    string? GetJwtIdFromToken(string token);
    bool IsTokenExpired(string token);
    DateTime GetTokenExpirationDate(string token);
    DateTime GetAccessTokenExpiryTime();
    Task<bool> IsTokenBlacklistedAsync(string jti);
    Task BlacklistTokenAsync(string jti, DateTime expiry);
}