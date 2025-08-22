using Authentication.Application.Commands;
using Authentication.Application.Interfaces;
using Authentication.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authentication.Application.Handlers;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ILogger<LogoutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Logout attempt");

            if (request.LogoutFromAllDevices && !string.IsNullOrEmpty(request.UserId))
            {
                Guid.TryParse(request.UserId, out Guid userId);
                // Revoke all refresh tokens for the user
                await _unitOfWork.RefreshTokensRepository.RevokeAllByUserIdAsync(userId);
                
                // Deactivate all user sessions
                await _unitOfWork.UserSessionsRepository.DeactivateAllByUserIdAsync(userId);
                
                _logger.LogInformation("Logged out from all devices for user: {UserId}", userId);
            }
            else if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                // Revoke specific refresh token
                var refreshToken = await _unitOfWork.RefreshTokensRepository.GetByTokenAsync(request.RefreshToken);
                
                if (refreshToken != null)
                {
                    refreshToken.MarkAsRevoked();
                    _logger.LogInformation("Refresh token revoked for user: {UserId}", refreshToken.UserId);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Logout successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout");
            return false;
        }
    }
}