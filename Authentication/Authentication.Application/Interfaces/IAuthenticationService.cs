using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Authentication.Domain.Enums;

namespace Authentication.Application.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(LogoutRequest request, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, string tokenType, CancellationToken cancellationToken = default);
}