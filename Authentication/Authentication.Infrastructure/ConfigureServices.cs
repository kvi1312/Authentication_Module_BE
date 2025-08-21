using Authentication.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.ConfigureUserDbContext(configuration);
        return services;
    }

    private static IServiceCollection ConfigureUserDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection(nameof(DbSettings)).Get<DbSettings>();

        if (dbSettings == null || string.IsNullOrWhiteSpace(dbSettings.ConnectionString))
            throw new InvalidOperationException("DbSettings:ConnectionString is not configured properly.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dbSettings.ConnectionString));

        return services;
    }
}
