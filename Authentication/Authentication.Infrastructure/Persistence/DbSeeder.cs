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
            if (context.Database.IsNpgsql())
            {
                _logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync();

                await CreateRoles(context);
                await CreateAdminUser(context);

                _logger.LogInformation("Seeding Authentication data completed!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task CreateRoles(AppDbContext context)
    {
        if (!context.Roles.Any())
        {
            _logger.LogInformation("Creating default roles...");

            await context.Roles.AddRangeAsync(
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Description = "admin",
                    UserType = UserType.Admin
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "EndUser",
                    UserType = UserType.EndUser
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Partner",
                    UserType = UserType.Partner
                }
            );

            await context.SaveChangesAsync();
            _logger.LogInformation("Roles created successfully");
        }
    }

    private async Task CreateAdminUser(AppDbContext context)
    {
        if (!context.Users.Any(u => u.Username == "admin"))
        {
            _logger.LogInformation("Creating admin user...");

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                _logger.LogError("Admin role not found, cannot create admin user");
                return;
            }

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("a2601197!", 12),
                Email = "admin@gmail.com",
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            };

            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();

            _logger.LogInformation("Admin user created successfully - Username: {Username}", adminUser.Username);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}