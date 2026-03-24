using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class ObligatorioDbContext : DbContext
{
    public DbSet<User> User { get; set; }
    public DbSet<UserRole> UserRole { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<Session> Sessions { get; set; } = default!;

    public ObligatorioDbContext(DbContextOptions<ObligatorioDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Entities
        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Surname).IsRequired();
            e.Property(x => x.Email).IsRequired();
            e.Property(x => x.Password).IsRequired();
            e.Property(x => x.DateOfBirth).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<Role>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<UserRole>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
        });

        b.Entity<Session>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).IsRequired();
            e.Property(x => x.UserId).IsRequired();

            e.HasIndex(x => x.Token).IsUnique();
        });

        // Entity relations
        b.Entity<UserRole>(e =>
        {
            e.HasOne(ur => ur.User)
             .WithMany()
             .HasForeignKey(ur => ur.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ur => ur.Role)
             .WithMany()
             .HasForeignKey(ur => ur.RoleId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Session>(e =>
        {
            e.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
