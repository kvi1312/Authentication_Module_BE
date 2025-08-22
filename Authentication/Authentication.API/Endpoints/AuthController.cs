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

    [HttpPost("login/{userType}")]
    public async Task<ActionResult<LoginResponse>> Login(UserType userType, LoginRequest request)
    {
        var command = new LoginCommand
        {
            Username = request.Username,
            Password = request.Password,
            UserType = userType,
            RememberMe = request.RememberMe
        };

        var result = await _mediator.Send(command);
        
        if (!result.Success) return BadRequest(result);

        if (!request.RememberMe) return Ok(result);
        
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = result.ExpiresAt
        };
            
        Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);

        return Ok(result);
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand { RefreshToken = request.RefreshToken };
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var command = new LogoutCommand { RefreshToken = request.RefreshToken };
        var result = await _mediator.Send(command);
        Response.Cookies.Delete("refreshToken");
        return result ? Ok(new { message = "Logged out successfully" }) : BadRequest();
    }
}