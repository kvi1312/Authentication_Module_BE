using Authentication.Application.Commands;
using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Queries;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Authentication.API.Endpoints;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public UserController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserManagementResponse>> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid user ID");
        }

        var query = new GetUserByIdQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UpdateUserProfileResponse>> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid user ID");
        }

        var command = _mapper.Map<UpdateUserProfileCommand>(request);
        command.UserId = userId;

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<UsersListResponse>> GetUsers([FromQuery] GetUsersRequest request)
    {
        var query = _mapper.Map<GetUsersQuery>(request);
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{userId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<UserManagementResponse>> GetUser(Guid userId)
    {
        var query = new GetUserByIdQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("{userId:guid}/roles/add")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<UserManagementResponse>> AddUserRoles(Guid userId, [FromBody] AddUserRoleRequest request)
    {
        var command = _mapper.Map<AddUserRoleCommand>(request);
        command.UserId = userId;

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{userId:guid}/roles/remove")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<UserManagementResponse>> RemoveUserRoles(Guid userId, [FromBody] RemoveUserRoleRequest request)
    {
        var command = _mapper.Map<RemoveUserRoleCommand>(request);
        command.UserId = userId;

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
