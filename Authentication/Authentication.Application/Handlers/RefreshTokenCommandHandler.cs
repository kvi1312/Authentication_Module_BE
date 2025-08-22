using Authentication.Application.Commands;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Interfaces;
using Authentication.Domain.Entities;
using Authentication.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authentication.Application.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Refresh token attempt");

            var storedRefreshToken = await _unitOfWork.RefreshTokensRepository.GetByTokenAsync(request.RefreshToken);
            
            if (storedRefreshToken == null || !storedRefreshToken.IsValid())
            {
                _logger.LogWarning("Invalid or expired refresh token");
                return new RefreshTokenResponse 
                { 
                    Success = false, 
                    Message = "Invalid or expired refresh token" 
                };
            }

            // Get user with roles
            var user = await _unitOfWork.UserRepository.GetWithRolesAsync(storedRefreshToken.UserId);
            
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("User not found or inactive for refresh token");
                return new RefreshTokenResponse 
                { 
                    Success = false, 
                    Message = "User not found or inactive" 
                };
            }

            // Mark old refresh token as used
            storedRefreshToken.MarkAsUsed();

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var jwtId = _jwtService.GetJwtIdFromToken(newAccessToken);

            if (string.IsNullOrEmpty(jwtId))
            {
                _logger.LogError("Failed to extract JWT ID from new access token");
                return new RefreshTokenResponse 
                { 
                    Success = false, 
                    Message = "Token generation failed" 
                };
            }

            // Create new refresh token entity
            var newRefreshTokenEntity = RefreshToken.Create(
                newRefreshToken,
                jwtId,
                user.Id,
                TimeSpan.FromDays(7) // Default refresh token validity
            );

            await _unitOfWork.RefreshTokensRepository.AddAsync(newRefreshTokenEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Token refresh successful for user: {UserId}", user.Id);

            return new RefreshTokenResponse
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = newRefreshTokenEntity.ExpiresAt,
                Message = "Token refreshed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token refresh");
            return new RefreshTokenResponse 
            { 
                Success = false, 
                Message = "An error occurred during token refresh" 
            };
        }
    }
}