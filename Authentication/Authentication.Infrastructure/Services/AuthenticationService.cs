using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Interfaces;
using Authentication.Domain.Enums;

namespace Authentication.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    public Task<LoginResponse> LoginAsync(LoginRequest request, UserType userType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LogoutAsync(LogoutRequest request, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ValidateTokenAsync(string token, string tokenType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}