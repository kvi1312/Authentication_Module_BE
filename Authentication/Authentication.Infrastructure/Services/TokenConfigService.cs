using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Interfaces;
using Authentication.Domain.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authentication.Infrastructure.Services;

public class TokenConfigService : ITokenConfigService
{
    private readonly IOptionsMonitor<JwtSettings> _jwtOptions;
    private readonly ILogger<TokenConfigService> _logger;
    private static JwtSettings? _runtimeSettings;

    public TokenConfigService(IOptionsMonitor<JwtSettings> jwtOptions, ILogger<TokenConfigService> logger)
    {
        _jwtOptions = jwtOptions;
        _logger = logger;
    }

    public TokenConfigResponse GetCurrentConfig()
    {
        var settings = GetCurrentSettings();

        return new TokenConfigResponse
        {
            AccessTokenExpiryMinutes = settings.ExpiryMinutes,
            RefreshTokenExpiryDays = settings.RefreshTokenExpiryDays,
            RememberMeTokenExpiryDays = settings.RememberMeTokenExpiryDays,
            AccessTokenExpiryDisplay = FormatDuration(TimeSpan.FromMinutes(settings.ExpiryMinutes)),
            RefreshTokenExpiryDisplay = FormatDuration(TimeSpan.FromDays(settings.RefreshTokenExpiryDays)),
            RememberMeTokenExpiryDisplay = FormatDuration(TimeSpan.FromDays(settings.RememberMeTokenExpiryDays))
        };
    }

    public Task<TokenConfigResponse> UpdateConfigAsync(UpdateTokenConfigRequest request)
    {
        var settings = GetCurrentSettings();

        if (request.AccessTokenExpiryMinutes.HasValue)
        {
            var minutes = Math.Max(1, Math.Min(60, request.AccessTokenExpiryMinutes.Value));
            settings.ExpiryMinutes = minutes;
            _logger.LogInformation("Updated AccessToken expiry to {Minutes} minutes", minutes);
        }

        if (request.RefreshTokenExpiryDays.HasValue)
        {
            var days = Math.Max(0.01, Math.Min(7, request.RefreshTokenExpiryDays.Value));
            settings.RefreshTokenExpiryDays = days;
            _logger.LogInformation("Updated RefreshToken expiry to {Days} days", days);
        }

        if (request.RememberMeTokenExpiryDays.HasValue)
        {
            var days = Math.Max(0.1, Math.Min(30, request.RememberMeTokenExpiryDays.Value));
            settings.RememberMeTokenExpiryDays = days;
            _logger.LogInformation("Updated RememberMeToken expiry to {Days} days", days);
        }

        _runtimeSettings = settings;

        return Task.FromResult(GetCurrentConfig());
    }

    public Task ResetToDefaultAsync()
    {
        _runtimeSettings = null;
        _logger.LogInformation("Reset token configuration to default values");
        return Task.CompletedTask;
    }

    private JwtSettings GetCurrentSettings()
    {
        if (_runtimeSettings != null)
            return _runtimeSettings;

        var originalSettings = _jwtOptions.CurrentValue;
        return new JwtSettings
        {
            SecretKey = originalSettings.SecretKey,
            Issuer = originalSettings.Issuer,
            Audience = originalSettings.Audience,
            ExpiryMinutes = originalSettings.ExpiryMinutes,
            RefreshTokenExpiryDays = originalSettings.RefreshTokenExpiryDays,
            RememberMeTokenExpiryDays = originalSettings.RememberMeTokenExpiryDays,
            ValidateIssuer = originalSettings.ValidateIssuer,
            ValidateAudience = originalSettings.ValidateAudience,
            ValidateLifetime = originalSettings.ValidateLifetime,
            ValidateIssuerSigningKey = originalSettings.ValidateIssuerSigningKey,
            ClockSkewSeconds = originalSettings.ClockSkewSeconds
        };
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
            return $"{duration.TotalDays:F1} days";
        if (duration.TotalHours >= 1)
            return $"{duration.TotalHours:F1} hours";
        return $"{duration.TotalMinutes:F0} minutes";
    }

    public static JwtSettings GetRuntimeSettings(IOptionsMonitor<JwtSettings> options)
    {
        return _runtimeSettings ?? options.CurrentValue;
    }
}
