using Authentication.Domain.Entities;
using Authentication.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;
namespace Authentication.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options, ILogger logger) : DbContext(options)
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException("Missing logger configuration");

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<RememberMeToken> RememberMeTokens { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Name).IsUnique();
        });

        // UserRole
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId);
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.JwtId);

            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId);
        });

        // RememberMeToken
        modelBuilder.Entity<RememberMeToken>(entity =>
        {
            entity.HasKey(rmt => rmt.Id);
            entity.HasIndex(rmt => rmt.TokenHash);

            entity.HasOne(rmt => rmt.User)
                  .WithMany()
                  .HasForeignKey(rmt => rmt.UserId);
        });

        // UserSession
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(us => us.Id);
            entity.HasIndex(us => us.SessionId).IsUnique();

            entity.HasOne(us => us.User)
                  .WithMany(u => u.UserSessions)
                  .HasForeignKey(us => us.UserId);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var modified = ChangeTracker.Entries()
                                    .Where(entity => entity.State == EntityState.Added || entity.State == EntityState.Modified || entity.State == EntityState.Deleted);

        foreach (var item in modified)
        {
            switch (item.State)
            {
                case EntityState.Added:
                    if (item.Entity is IDateTracking addedEntity)
                    {
                        addedEntity.CreatedDate = DateTime.UtcNow;
                        item.State = EntityState.Added;
                    }
                    break;

                case EntityState.Modified:
                    var idField = Entry(item.Entity).Property("Id");
                    idField.IsModified = false;
                    if (item.Entity is IDateTracking modifiedEntity)
                    {
                        modifiedEntity.LastModifiedDate = DateTime.UtcNow;
                        item.State = EntityState.Modified;
                    }
                    break;
                default:
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}