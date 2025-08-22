using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authentication.Application.Interfaces;
using Authentication.Domain.Configurations;
using Authentication.Domain.Entities;
using Authentication.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Authentication.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IOptionsMonitor<JwtSettings> _jwtOptions;
    private readonly ILogger<JwtService> _logger;
    private static readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();

    public JwtService(IOptionsMonitor<JwtSettings> jwtOptions, ILogger<JwtService> logger)
    {
        _jwtOptions = jwtOptions;
        _logger = logger;
    }

    private JwtSettings GetCurrentSettings() => TokenConfigService.GetRuntimeSettings(_jwtOptions);

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var settings = GetCurrentSettings();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var settings = GetCurrentSettings();
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                ValidateLifetime = false
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating expired token");
            return null;
        }
    }

    public string? GetJwtIdFromToken(string token)
    {
        try
        {
            var jwt = GetToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading JWT ID from token");
            return null;
        }
    }

    public bool IsTokenExpired(string token)
    {
        try
        {
            var jwt = GetToken(token);
            return jwt.ValidTo < DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token expiration");
            return true;
        }
    }

    public DateTime GetTokenExpirationDate(string token)
    {
        try
        {
            var jwt = GetToken(token);
            return jwt.ValidTo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token expiration date");
            return DateTime.MinValue;
        }
    }

    public Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        return Task.FromResult(_blacklistedTokens.ContainsKey(jti));
    }

    public Task BlacklistTokenAsync(string jti, DateTime expiry)
    {
        _blacklistedTokens.TryAdd(jti, expiry);
        return Task.CompletedTask;
    }

    private JwtSecurityToken GetToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.ReadJwtToken(token);
    }
}