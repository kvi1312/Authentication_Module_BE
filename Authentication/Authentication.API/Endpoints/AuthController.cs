using Authentication.Application.Commands;
using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Authentication.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Endpoints;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var command = new LoginCommand
        {
            Username = request.Username,
            Password = request.Password,
            RememberMe = request.RememberMe,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            DeviceInfo = Request.Headers["User-Agent"].ToString()
        };

        var result = await _mediator.Send(command);

        if (!result.Success) return BadRequest(result);

        // Set secure cookies for remember me functionality
        if (request.RememberMe && !string.IsNullOrEmpty(result.RefreshToken))
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                Path = "/"
            };

            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);


            if (!string.IsNullOrEmpty(result.RememberMeToken))
            {
                Response.Cookies.Append("rememberMe", result.RememberMeToken, cookieOptions);
            }
        }

        var safeResult = new LoginResponse
        {
            Success = result.Success,
            Message = result.Message,
            AccessToken = result.AccessToken,
            ExpiresAt = result.ExpiresAt,
            User = result.User
        };

        return Ok(safeResult);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest? request = null)
    {
        string? refreshToken = request?.RefreshToken;

        if (string.IsNullOrEmpty(refreshToken))
        {
            refreshToken = Request.Cookies["refreshToken"];
        }

        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        var command = new RefreshTokenCommand
        {
            RefreshToken = refreshToken,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            DeviceInfo = Request.Headers["User-Agent"].ToString()
        };

        var result = await _mediator.Send(command);

        if (result.Success && !string.IsNullOrEmpty(result.RefreshToken))
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.ExpiresAt,
                Path = "/"
            };

            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
    {
        string? refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];
        string? accessToken = null;
        if (Request.Headers.Authorization.Any())
        {
            var authHeader = Request.Headers.Authorization.First();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                accessToken = authHeader.Substring("Bearer ".Length).Trim();
            }
        }

        var command = new LogoutCommand
        {
            RefreshToken = refreshToken,
            AccessToken = accessToken
        };

        var result = await _mediator.Send(command);
        Response.Cookies.Delete("refreshToken");
        Response.Cookies.Delete("rememberMe");

        return result ? Ok(new { message = "Logged out successfully" }) : BadRequest(new { message = "Logout failed" });
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            DeviceInfo = Request.Headers["User-Agent"].ToString()
        };

        var result = await _mediator.Send(command);

        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }
}