using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class ObligatorioDbContext : DbContext
{
    public DbSet<User> User { get; set; }
    public DbSet<UserRole> UserRole { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<Attraction> Attraction { get; set; }
    public DbSet<Incident> Incident { get; set; }
    public DbSet<SpecialEvent> SpecialEvent { get; set; }
    public DbSet<Ticket> Ticket { get; set; }
    public DbSet<AccessRecord> AccessRecord { get; set; }
    public DbSet<Mission> Mission { get; set; }
    public DbSet<Achievement> Achievement { get; set; }
    public DbSet<Reward> Reward { get; set; }
    public DbSet<MissionCompletion> MissionCompletion { get; set; }
    public DbSet<Redemption> Redemption { get; set; }
    public DbSet<UnlockLog> UnlockLog { get; set; }
    public DbSet<PointsAward> PointsAward { get; set; }
    public DbSet<Session> Sessions { get; set; } = default!;
    public DbSet<ScoringStrategyMeta> ScoringStrategyMeta { get; set; }
    public DbSet<Maintenance> Maintenance { get; set; }

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
            e.Property(x => x.Membership).IsRequired();
            e.Property(x => x.Points).IsRequired();
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

        b.Entity<Attraction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Type).IsRequired();
            e.Property(x => x.MinAge).IsRequired();
            e.Property(x => x.MaxCapacity).IsRequired();
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.Enabled).IsRequired();
        });

        b.Entity<Incident>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.ReportedAt).IsRequired();
            e.Property(x => x.Resolved).IsRequired();
        });

        b.Entity<SpecialEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.StartDate).IsRequired();
            e.Property(x => x.EndDate).IsRequired();
            e.Property(x => x.Capacity).IsRequired();
            e.Property(x => x.ExtraPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
        });

        b.Entity<Ticket>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.QrCode).IsRequired();
            e.Property(x => x.VisitDate).IsRequired();
            e.Property(x => x.Type).IsRequired();
            e.HasIndex(x => x.QrCode).IsUnique();
        });

        b.Entity<AccessRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.InAt).IsRequired();
        });

        b.Entity<Mission>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired();
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.BasePoints).IsRequired();
        });

        b.Entity<Achievement>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Description).IsRequired();
        });

        b.Entity<Reward>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.CostPoints).IsRequired();
        });

        b.Entity<MissionCompletion>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DateCompleted).IsRequired();
            e.Property(x => x.Points).IsRequired();
            e.HasIndex(x => new { x.UserId, x.MissionId }).IsUnique();
        });

        b.Entity<Redemption>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DateClaimed).IsRequired();
            e.Property(x => x.CostPoints).IsRequired();
        });

        b.Entity<UnlockLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DateUnlocked).IsRequired();
            e.HasIndex(x => new { x.UserId, x.AchievementId }).IsUnique();
        });

        b.Entity<PointsAward>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Points).IsRequired();
            e.Property(x => x.Reason).IsRequired();
            e.Property(x => x.StrategyId).IsRequired();
            e.Property(x => x.At).IsRequired();
            e.HasIndex(x => new { x.UserId, x.At });
        });

        b.Entity<Session>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).IsRequired();
            e.Property(x => x.UserId).IsRequired();

            e.HasIndex(x => x.Token).IsUnique();
        });

        b.Entity<ScoringStrategyMeta>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Name)
                .IsRequired();

            e.Property(x => x.IsActive)
                .IsRequired();

            e.Property(x => x.IsDeleted)
                .IsRequired();

            e.Property(x => x.CreatedOn)
                .IsRequired();

            e.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(450);

            e.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(450);

            // No duplicates for the physical file location+name
            e.HasIndex(x => new { x.FilePath, x.FileName }).IsUnique();
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

        b.Entity<Incident>(e =>
        {
            e.HasOne(i => i.Attraction)
             .WithMany()
             .HasForeignKey(i => i.AttractionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Ticket>(e =>
        {
            e.HasOne(t => t.Owner)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(t => t.SpecialEvent)
                .WithMany()
                .HasForeignKey(t => t.SpecialEventId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull); // <- was Cascade
        });

        b.Entity<AccessRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.InAt).IsRequired();

            e.HasOne(a => a.Ticket)
                .WithMany()
                .HasForeignKey(a => a.TicketId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.Attraction)
                .WithMany()
                .HasForeignKey(a => a.AttractionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.SpecialEvent)
                .WithMany()
                .HasForeignKey(a => a.SpecialEventId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull); // <- was Cascade
        });

        b.Entity<MissionCompletion>(e =>
        {
            e.HasOne(mc => mc.User)
             .WithMany()
             .HasForeignKey(mc => mc.UserId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(mc => mc.Mission)
             .WithMany()
             .HasForeignKey(mc => mc.MissionId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Redemption>(e =>
        {
            e.HasOne(r => r.User)
             .WithMany()
             .HasForeignKey(r => r.UserId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.Reward)
             .WithMany()
             .HasForeignKey(r => r.RewardId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<UnlockLog>(e =>
        {
            e.HasOne(ul => ul.User)
             .WithMany()
             .HasForeignKey(ul => ul.UserId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ul => ul.Achievement)
             .WithMany()
             .HasForeignKey(ul => ul.AchievementId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<PointsAward>(e =>
        {
            e.HasOne(pa => pa.User)
                .WithMany()
                .HasForeignKey(pa => pa.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne<ScoringStrategyMeta>()
                .WithMany()
                .HasForeignKey(pa => pa.StrategyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Session>(e =>
        {
            e.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Maintenance>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StartAt).IsRequired();
            e.Property(x => x.DurationMinutes).IsRequired();
            e.Property(x => x.Description).IsRequired();

            e.HasOne(x => x.Attraction)
             .WithMany()
             .HasForeignKey(x => x.AttractionId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.AttractionId, x.StartAt });
        });
    }
}
