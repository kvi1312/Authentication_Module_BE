using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Interfaces;
using Authentication.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Endpoints;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = AuthenticationConstants.Roles.SuperAdmin)]
public class AdminController : ControllerBase
{
    private readonly ITokenConfigService _tokenConfigService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ITokenConfigService tokenConfigService, ILogger<AdminController> logger)
    {
        _tokenConfigService = tokenConfigService;
        _logger = logger;
    }

    /// <summary>
    /// Get current token configuration settings
    /// </summary>
    [HttpGet("token-config")]
    public IActionResult GetTokenConfig()
    {
        try
        {
            var config = _tokenConfigService.GetCurrentConfig();
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token configuration");
            return StatusCode(500, new { message = "Failed to get token configuration" });
        }
    }

    /// <summary>
    /// Update token configuration - BACKDOOR for demo purposes
    /// </summary>
    /// <param name="request">Token configuration update request</param>
    /// <remarks>
    /// Limits:
    /// - AccessToken: 1-60 minutes
    /// - RefreshToken: 0.01-7 days (about 15 minutes to 1 week)
    /// - RememberMe: 0.1-30 days (about 2.4 hours to 1 month)
    /// </remarks>
    [HttpPut("token-config")]
    public async Task<IActionResult> UpdateTokenConfig([FromBody] UpdateTokenConfigRequest request)
    {
        try
        {
            var adminUser = User.Identity?.Name;
            _logger.LogWarning("Admin {AdminUser} is updating token configuration: {@Request}", adminUser, request);

            var updatedConfig = await _tokenConfigService.UpdateConfigAsync(request);

            return Ok(new
            {
                message = "Token configuration updated successfully",
                config = updatedConfig,
                warning = "This is a runtime configuration change. Restart the application to use original settings."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating token configuration");
            return StatusCode(500, new { message = "Failed to update token configuration" });
        }
    }

    /// <summary>
    /// Reset token configuration to default values
    /// </summary>
    [HttpPost("token-config/reset")]
    public async Task<IActionResult> ResetTokenConfig()
    {
        try
        {
            var adminUser = User.Identity?.Name;
            _logger.LogWarning("Admin {AdminUser} is resetting token configuration to defaults", adminUser);

            await _tokenConfigService.ResetToDefaultAsync();
            var config = _tokenConfigService.GetCurrentConfig();

            return Ok(new
            {
                message = "Token configuration reset to default values",
                config = config
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting token configuration");
            return StatusCode(500, new { message = "Failed to reset token configuration" });
        }
    }

    /// <summary>
    /// Quick demo presets for token lifetimes
    /// </summary>
    [HttpPost("token-config/preset/{presetName}")]
    public async Task<IActionResult> ApplyPreset(string presetName)
    {
        try
        {
            var adminUser = User.Identity?.Name;
            _logger.LogWarning("Admin {AdminUser} is applying token preset: {PresetName}", adminUser, presetName);

            UpdateTokenConfigRequest preset = presetName.ToLower() switch
            {
                "very-short" => new UpdateTokenConfigRequest
                {
                    AccessTokenExpiryMinutes = 2,
                    RefreshTokenExpiryDays = 0.02, // ~30 minutes
                    RememberMeTokenExpiryDays = 0.1 // ~2.4 hours
                },
                "short" => new UpdateTokenConfigRequest
                {
                    AccessTokenExpiryMinutes = 5,
                    RefreshTokenExpiryDays = 0.25, // 6 hours
                    RememberMeTokenExpiryDays = 1 // 1 day
                },
                "medium" => new UpdateTokenConfigRequest
                {
                    AccessTokenExpiryMinutes = 15,
                    RefreshTokenExpiryDays = 1, // 1 day
                    RememberMeTokenExpiryDays = 7 // 1 week
                },
                "long" => new UpdateTokenConfigRequest
                {
                    AccessTokenExpiryMinutes = 60,
                    RefreshTokenExpiryDays = 7, // 1 week
                    RememberMeTokenExpiryDays = 30 // 1 month
                },
                _ => throw new ArgumentException($"Unknown preset: {presetName}")
            };

            var updatedConfig = await _tokenConfigService.UpdateConfigAsync(preset);

            return Ok(new
            {
                message = $"Applied '{presetName}' preset successfully",
                preset = presetName,
                config = updatedConfig
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message, availablePresets = new[] { "very-short", "short", "medium", "long" } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying token preset: {PresetName}", presetName);
            return StatusCode(500, new { message = "Failed to apply preset" });
        }
    }

    /// <summary>
    /// Get all available presets
    /// </summary>
    [HttpGet("token-config/presets")]
    public IActionResult GetPresets()
    {
        var presets = new
        {
            very_short = new { access = "2 min", refresh = "30 min", remember = "2.4 hours" },
            @short = new { access = "5 min", refresh = "6 hours", remember = "1 day" },
            medium = new { access = "15 min", refresh = "1 day", remember = "1 week" },
            @long = new { access = "1 hour", refresh = "1 week", remember = "1 month" }
        };

        return Ok(new
        {
            message = "Available token lifetime presets",
            presets = presets,
            usage = "POST /api/admin/token-config/preset/{presetName}"
        });
    }
}
