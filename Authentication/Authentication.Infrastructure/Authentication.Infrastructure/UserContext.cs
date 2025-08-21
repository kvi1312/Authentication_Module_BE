using Authentication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;
namespace Authentication.Infrastructure;

public class UserContext(DbContextOptions<UserContext> options, ILogger logger) : DbContext(options)
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException("Missing logger configuration");

    public DbSet<User> Users { get; set; }
}