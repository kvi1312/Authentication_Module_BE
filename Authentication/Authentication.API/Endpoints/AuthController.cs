using Authentication.Application.Commands;
using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
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

        if (!string.IsNullOrEmpty(result.RefreshToken))
        {
            // Normal Login : No Expires = Session cookie (deleted when browser closes)
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };

            // Remember Me = Persistent cookie
            if (request.RememberMe)
            {
                refreshCookieOptions.Expires = result.RefreshTokenExpiresAt;
            }

            Response.Cookies.Append("refreshToken", System.Web.HttpUtility.UrlEncode(result.RefreshToken), refreshCookieOptions);
        }

        // Set remember me token if requested
        if (request.RememberMe && !string.IsNullOrEmpty(result.RememberMeToken))
        {
            var rememberMeCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = result.RememberMeTokenExpiresAt,
                Path = "/"
            };

            Response.Cookies.Append("rememberMe", System.Web.HttpUtility.UrlEncode(result.RememberMeToken), rememberMeCookieOptions);
        }

        var safeResult = new LoginResponse
        {
            Success = result.Success,
            Message = result.Message,
            AccessToken = result.AccessToken,
            RefreshToken = null,
            RememberMeToken = null,
            AccessTokenExpiresAt = result.AccessTokenExpiresAt,
            RefreshTokenExpiresAt = result.RefreshTokenExpiresAt,
            RememberMeTokenExpiresAt = result.RememberMeTokenExpiresAt,
            User = result.User,
            SessionId = result.SessionId
        };

        return Ok(safeResult);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest? request = null)
    {
        string? refreshToken = request?.RefreshToken;

        if (string.IsNullOrEmpty(refreshToken))
        {
            refreshToken = Request.Cookies["refreshToken"];
        }

        // If no refresh token, try remember me token as fallback
        if (string.IsNullOrEmpty(refreshToken))
        {
            refreshToken = Request.Cookies["rememberMe"];
        }

        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { message = "Refresh token or remember me token is required" });
        }

        refreshToken = System.Web.HttpUtility.UrlDecode(refreshToken);

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
                Path = "/"
            };

            if (result.IsRememberMe)
            {
                cookieOptions.Expires = result.RefreshTokenExpiresAt;
            }

            Response.Cookies.Append("refreshToken", System.Web.HttpUtility.UrlEncode(result.RefreshToken), cookieOptions);
        }

        var safeResult = new RefreshTokenResponse
        {
            Success = result.Success,
            Message = result.Message,
            AccessToken = result.AccessToken,
            RefreshToken = null,
            AccessTokenExpiresAt = result.AccessTokenExpiresAt,
            RefreshTokenExpiresAt = result.RefreshTokenExpiresAt,
            IsRememberMe = result.IsRememberMe
        };

        return result.Success ? Ok(safeResult) : BadRequest(result);
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