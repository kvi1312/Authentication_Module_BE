using Authentication.Application.Commands;
using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Response;
using Authentication.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authentication.Application.Handlers;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UpdateUserProfileResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetWithRolesAsync(request.UserId);
            if (user == null)
            {
                return new UpdateUserProfileResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var existingUserWithEmail = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);
            if (existingUserWithEmail != null && existingUserWithEmail.Id != request.UserId)
            {
                return new UpdateUserProfileResponse
                {
                    Success = false,
                    Message = "Email is already in use by another user"
                };
            }

            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.LastModifiedDate = DateTimeOffset.UtcNow;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);

            _logger.LogInformation("User profile updated successfully for user {UserId}", request.UserId);

            return new UpdateUserProfileResponse
            {
                Success = true,
                Message = "Profile updated successfully",
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile for user {UserId}", request.UserId);
            return new UpdateUserProfileResponse
            {
                Success = false,
                Message = "An error occurred while updating profile"
            };
        }
    }
}
