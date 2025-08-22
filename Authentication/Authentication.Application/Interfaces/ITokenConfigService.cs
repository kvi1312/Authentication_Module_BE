using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;

namespace Authentication.Application.Interfaces;

public interface ITokenConfigService
{
    TokenConfigResponse GetCurrentConfig();
    Task<TokenConfigResponse> UpdateConfigAsync(UpdateTokenConfigRequest request);
    Task ResetToDefaultAsync();
}
