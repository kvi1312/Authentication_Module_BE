using Authentication.Application.Common.Models;

namespace Authentication.API.Extensions;

public static class ServicesExtension
{
    public static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection(nameof(DbSettings)).Get<DbSettings>() ??
                         throw new InvalidOperationException(
                             $"'{nameof(DbSettings)}' section is missing in configuration");
        services.AddSingleton(dbSettings);
        return services;
    }
}