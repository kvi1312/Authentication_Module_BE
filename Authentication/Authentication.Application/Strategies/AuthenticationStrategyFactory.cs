using Authentication.Domain.Enums;

namespace Authentication.Application.Strategies;

public class AuthenticationStrategyFactory : IAuthenticationStrategyFactory
{
    private readonly IEnumerable<IAuthenticationStrategy> _strategies;

    public AuthenticationStrategyFactory(IEnumerable<IAuthenticationStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IAuthenticationStrategy GetStrategy(UserType userType)
    {
        var strategy = _strategies.FirstOrDefault(s => s.UserType == userType);
        
        if (strategy == null)
        {
            throw new ArgumentException($"No authentication strategy found for user type: {userType}");
        }

        return strategy;
    }
    public IEnumerable<IAuthenticationStrategy> GetAllStrategies()
    {
        return _strategies;
    }
}