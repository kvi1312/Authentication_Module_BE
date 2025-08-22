using Authentication.Domain.Enums;

namespace Authentication.Application.Strategies;

public interface IAuthenticationStrategyFactory
{
    IAuthenticationStrategy GetStrategy(UserType userType);
    IEnumerable<IAuthenticationStrategy> GetAllStrategies();
}