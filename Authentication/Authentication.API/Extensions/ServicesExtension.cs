
using Authentication.Application.Mappings;
using Authentication.Domain.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddAutoMapper(typeof(MappingProfile));

        return services;
    }

    public static IServiceCollection ConfigAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ??
                          throw new InvalidOperationException("JWT settings are not configured properly");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = jwtSettings.ValidateLifetime,
                    ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkewSeconds)
                };
            });
        return services;
    }
}