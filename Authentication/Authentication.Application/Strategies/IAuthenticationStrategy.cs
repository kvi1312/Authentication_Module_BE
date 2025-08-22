using Authentication.Domain.Entities;
using Authentication.Domain.Enums;

namespace Authentication.Application.Strategies;

public interface IAuthenticationStrategy
{
    UserType UserType { get; }
    Task<User?> ValidateAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetAdditionalClaimsAsync(User user, CancellationToken cancellationToken = default);
}