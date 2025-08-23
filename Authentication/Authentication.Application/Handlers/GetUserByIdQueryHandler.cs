using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Queries;
using Authentication.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authentication.Application.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserManagementResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetUserByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserManagementResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetWithRolesAsync(request.UserId);
            if (user == null)
            {
                return new UserManagementResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var userDto = _mapper.Map<UserDto>(user);

            return new UserManagementResponse
            {
                Success = true,
                Message = "User retrieved successfully",
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", request.UserId);
            return new UserManagementResponse
            {
                Success = false,
                Message = "An error occurred while retrieving user"
            };
        }
    }
}
