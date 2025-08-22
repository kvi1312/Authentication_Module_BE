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
                
                await _unitOfWork.RefreshTokensRepository.RevokeAllByUserIdAsync(userId);

                await _unitOfWork.UserSessionsRepository.DeactivateAllByUserIdAsync(userId);

                _logger.LogInformation("Logged out from all devices for user: {UserId}", userId);
            }
            else
            {
                // Revoke specific refresh token if provided
                if (!string.IsNullOrEmpty(request.RefreshToken))
                {
                    var refreshToken = await _unitOfWork.RefreshTokensRepository.GetByTokenAsync(request.RefreshToken);

                    if (refreshToken != null)
                    {
                        refreshToken.MarkAsRevoked();
                        _logger.LogInformation("Refresh token revoked for user: {UserId}", refreshToken.UserId);
                    }
                }

                // Blacklist access token if provided
                if (!string.IsNullOrEmpty(request.AccessToken))
                {
                    var jwtId = _jwtService.GetJwtIdFromToken(request.AccessToken);
                    if (!string.IsNullOrEmpty(jwtId))
                    {
                        var expiry = _jwtService.GetTokenExpirationDate(request.AccessToken);
                        await _jwtService.BlacklistTokenAsync(jwtId, expiry);
                        _logger.LogInformation("Access token blacklisted with JTI: {JwtId}", jwtId);
                    }
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