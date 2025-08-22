using Authentication.Application.Interfaces;
using Authentication.Domain.Constants;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Authentication.Infrastructure.Persistence;

public class DbSeeder : IHostedService
{
    private readonly ILogger<DbSeeder> _logger;
    private readonly IServiceProvider _serviceProvider;
    public DbSeeder(ILogger<DbSeeder> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

            if (context.Database.IsNpgsql())
            {
                _logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync(cancellationToken);

                await SeedRolesAsync(context, cancellationToken);
                await SeedDefaultUsersAsync(context, passwordService, cancellationToken);
                await CleanupExpiredTokensAsync(context, cancellationToken);

                _logger.LogInformation("Database seeding completed successfully!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedRolesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seeding roles...");

        var existingRoles = await context.Roles.Select(r => r.Name).ToListAsync(cancellationToken);

        var rolesToAdd = new List<Role>();

        foreach (var userType in Enum.GetValues<UserType>())
        {
            if (DefaultRoles.UserTypeRoles.TryGetValue(userType, out var roles))
            {
                foreach (var (roleName, description) in roles)
                {
                    if (!existingRoles.Contains(roleName))
                    {
                        var role = Role.Create(roleName, description, userType);
                        rolesToAdd.Add(role);
                        _logger.LogInformation("Will create role: {RoleName} for user type: {UserType}", roleName, userType);
                    }
                }
            }
        }

        if (rolesToAdd.Any())
        {
            await context.Roles.AddRangeAsync(rolesToAdd, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully created {Count} roles", rolesToAdd.Count);
        }
        else
        {
            _logger.LogInformation("All roles already exist, skipping role seeding");
        }
    }

    private async Task SeedDefaultUsersAsync(AppDbContext context, IPasswordService passwordService, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seeding default users...");

        // Create Super Admin
        await CreateUserIfNotExists(context, passwordService, new UserSeedData
        {
            Username = "admin",
            Email = "admin@authmodule.com",
            Password = "Admin@123",
            FirstName = "System",
            LastName = "Administrator",
            RoleName = AuthenticationConstants.Roles.SuperAdmin,
            UserType = UserType.Admin
        }, cancellationToken);

        // Create default EndUser
        await CreateUserIfNotExists(context, passwordService, new UserSeedData
        {
            Username = "user",
            Email = "user@authmodule.com",
            Password = "User@123",
            FirstName = "Test",
            LastName = "User",
            RoleName = AuthenticationConstants.Roles.EndUser,
            UserType = UserType.EndUser
        }, cancellationToken);

        // Create default Partner
        await CreateUserIfNotExists(context, passwordService, new UserSeedData
        {
            Username = "partner",
            Email = "partner@authmodule.com",
            Password = "Partner@123",
            FirstName = "Test",
            LastName = "Partner",
            RoleName = AuthenticationConstants.Roles.Partner,
            UserType = UserType.Partner
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateUserIfNotExists(AppDbContext context, IPasswordService passwordService, UserSeedData userData, CancellationToken cancellationToken)
    {
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username == userData.Username, cancellationToken);

        if (existingUser == null)
        {
            _logger.LogInformation("Creating user: {Username}", userData.Username);

            var passwordHash = passwordService.HashPassword(userData.Password);
            var user = User.Create(userData.Username, userData.Email, passwordHash, userData.FirstName, userData.LastName);

            await context.Users.AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == userData.RoleName, cancellationToken);
            if (role != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedDate = DateTimeOffset.UtcNow
                };
                await context.UserRoles.AddAsync(userRole, cancellationToken);

                _logger.LogInformation("Created user: {Username} with role: {RoleName}, Password: {Password}",
                    userData.Username, userData.RoleName, userData.Password);
            }
            else
            {
                _logger.LogWarning("Role {RoleName} not found for user {Username}", userData.RoleName, userData.Username);
            }
        }
        else
        {
            _logger.LogInformation("User {Username} already exists, skipping creation", userData.Username);
        }
    }

    private async Task CleanupExpiredTokensAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cleaning up expired tokens...");

        // Cleanup expired refresh tokens
        var expiredRefreshTokens = await context.RefreshTokens
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expiredRefreshTokens.Any())
        {
            context.RefreshTokens.RemoveRange(expiredRefreshTokens);
            _logger.LogInformation("Removed {Count} expired refresh tokens", expiredRefreshTokens.Count);
        }

        // Cleanup expired remember me tokens
        var expiredRememberMeTokens = await context.RememberMeTokens
            .Where(rmt => rmt.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expiredRememberMeTokens.Any())
        {
            context.RememberMeTokens.RemoveRange(expiredRememberMeTokens);
            _logger.LogInformation("Removed {Count} expired remember me tokens", expiredRememberMeTokens.Count);
        }

        // Cleanup expired user sessions
        var expiredSessions = await context.UserSessions
            .Where(us => us.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expiredSessions.Any())
        {
            context.UserSessions.RemoveRange(expiredSessions);
            _logger.LogInformation("Removed {Count} expired user sessions", expiredSessions.Count);
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Token cleanup completed");
    }

    private class UserSeedData
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}