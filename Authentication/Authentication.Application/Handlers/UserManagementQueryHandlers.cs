using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Queries;
using Authentication.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authentication.Application.Handlers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, UsersListResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetUsersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UsersListResponse> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (users, totalCount) = await _unitOfWork.UserRepository.GetPagedUsersAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.UserType,
                request.RoleFilter);

            var userDtos = _mapper.Map<List<UserDto>>(users);

            return new UsersListResponse
            {
                Success = true,
                Message = "Users retrieved successfully",
                Users = userDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with pagination");
            return new UsersListResponse
            {
                Success = false,
                Message = "An error occurred while retrieving users",
                Users = new List<UserDto>(),
                TotalCount = 0,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
