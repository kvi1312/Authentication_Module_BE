using Authentication.Application.Common.Models;
using Authentication.Domain.Interfaces;
using Authentication.Domain.Interfaces.Repositories;
using Authentication.Infrastructure.Repositories;
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
        services.AddScoped<DbFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserServices, UserServices>();
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
